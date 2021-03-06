﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the Microsoft Reciprocal License (Ms-RL) license. See LICENSE file in the project root for full license information.

namespace IronPigeon.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Moq;
    using Validation;
    using Xunit;
    using Xunit.Abstractions;

    public class ChannelTests
    {
        private Mocks.LoggerMock logger;

        private CryptoSettings desktopCryptoProvider;

        public ChannelTests(ITestOutputHelper logger)
        {
            this.logger = new Mocks.LoggerMock(logger);
            this.desktopCryptoProvider = TestUtilities.CreateAuthenticCryptoProvider();
        }

        [Fact]
        public void DefaultCtor()
        {
            var channel = new Channel();
            Assert.Null(channel.CloudBlobStorage);
            Assert.NotNull(channel.CryptoServices);
            Assert.Null(channel.Endpoint);
        }

        [Fact]
        public void CtorParameters()
        {
            var blobProvider = new Mock<ICloudBlobStorageProvider>();
            var endpoint = new Mock<OwnEndpoint>();
            var channel = new Channel()
            {
                CloudBlobStorage = blobProvider.Object,
                Endpoint = endpoint.Object,
            };
            Assert.Same(channel.CloudBlobStorage, blobProvider.Object);
            Assert.Same(channel.Endpoint, endpoint.Object);
        }

        [Fact]
        public void HttpClient()
        {
            var channel = new Channel();
            Assert.Null(channel.HttpClient);
            var handler = new HttpClient();
            channel.HttpClient = handler;
            Assert.Same(channel.HttpClient, handler);
        }

        [Fact]
        public void PostAsyncBadArgs()
        {
            var channel = new Channel();
            Assert.Throws<ArgumentNullException>(() => channel.PostAsync(null, Valid.OneEndpoint, Valid.ExpirationUtc).GetAwaiter().GetResult());
            Assert.Throws<ArgumentNullException>(() => channel.PostAsync(Valid.Message, null, Valid.ExpirationUtc).GetAwaiter().GetResult());
            Assert.Throws<ArgumentException>(() => channel.PostAsync(Valid.Message, Valid.EmptyEndpoints, Valid.ExpirationUtc).GetAwaiter().GetResult());
            Assert.Throws<ArgumentException>(() => channel.PostAsync(Valid.Message, Valid.OneEndpoint, Invalid.ExpirationUtc).GetAwaiter().GetResult());
        }

        [Fact]
        public void PostAndReceiveAsync()
        {
            Task.Run(async delegate
            {
                var sender = Valid.GenerateOwnEndpoint();
                var receiver = Valid.GenerateOwnEndpoint();

                var cloudStorage = new Mocks.CloudBlobStorageProviderMock();
                var inboxMock = new Mocks.InboxHttpHandlerMock(new[] { receiver.PublicEndpoint });
                var cryptoProvider = new CryptoSettings(SecurityLevel.Minimum);

                var sentMessage = Valid.Message;
                await this.SendMessageAsync(cloudStorage, inboxMock, cryptoProvider, sender, receiver.PublicEndpoint, sentMessage);
                var messages = await this.ReceiveMessageAsync(cloudStorage, inboxMock, new CryptoSettings(SecurityLevel.Minimum), receiver);

                Assert.Equal(1, messages.Count);
                var receivedMessage = messages.Single();
                Assert.Equal(receivedMessage.Payload.ContentType, sentMessage.ContentType);
                Assert.Equal(receivedMessage.Payload.Content, sentMessage.Content);
            }).GetAwaiter().GetResult();
        }

        [Fact]
        public void PayloadReferenceTamperingTests()
        {
            Task.Run(async delegate
            {
                var sender = Valid.GenerateOwnEndpoint(this.desktopCryptoProvider);
                var receiver = Valid.GenerateOwnEndpoint(this.desktopCryptoProvider);

                for (int i = 0; i < 100; i++)
                {
                    var cloudStorage = new Mocks.CloudBlobStorageProviderMock();
                    var inboxMock = new Mocks.InboxHttpHandlerMock(new[] { receiver.PublicEndpoint });

                    var sentMessage = Valid.Message;
                    await this.SendMessageAsync(cloudStorage, inboxMock, this.desktopCryptoProvider, sender, receiver.PublicEndpoint, sentMessage);

                    // Tamper with the payload reference.
                    TestUtilities.ApplyFuzzing(inboxMock.Inboxes[receiver.PublicEndpoint][0].Item2, 1);

                    var receivedMessages =
                        await this.ReceiveMessageAsync(cloudStorage, inboxMock, this.desktopCryptoProvider, receiver, expectMessage: false);
                    Assert.Equal(0, receivedMessages.Count);
                }
            }).GetAwaiter().GetResult();
        }

        [Fact]
        public void PayloadTamperingTests()
        {
            Task.Run(async delegate
            {
                var sender = Valid.GenerateOwnEndpoint(this.desktopCryptoProvider);
                var receiver = Valid.GenerateOwnEndpoint(this.desktopCryptoProvider);

                for (int i = 0; i < 100; i++)
                {
                    var cloudStorage = new Mocks.CloudBlobStorageProviderMock();
                    var inboxMock = new Mocks.InboxHttpHandlerMock(new[] { receiver.PublicEndpoint });

                    var sentMessage = Valid.Message;
                    await this.SendMessageAsync(cloudStorage, inboxMock, this.desktopCryptoProvider, sender, receiver.PublicEndpoint, sentMessage);

                    // Tamper with the payload itself.
                    TestUtilities.ApplyFuzzing(cloudStorage.Blobs.Single().Value, 1);

                    var receivedMessages =
                        await this.ReceiveMessageAsync(cloudStorage, inboxMock, this.desktopCryptoProvider, receiver, expectMessage: false);
                    Assert.Equal(0, receivedMessages.Count);
                }
            }).GetAwaiter().GetResult();
        }

        private async Task SendMessageAsync(Mocks.CloudBlobStorageProviderMock cloudBlobStorage, Mocks.InboxHttpHandlerMock inboxMock, CryptoSettings cryptoProvider, OwnEndpoint sender, Endpoint receiver, Payload message)
        {
            Requires.NotNull(cloudBlobStorage, "cloudBlobStorage");
            Requires.NotNull(sender, "sender");
            Requires.NotNull(message, "message");

            var httpHandler = new Mocks.HttpMessageHandlerMock();

            cloudBlobStorage.AddHttpHandler(httpHandler);
            inboxMock.Register(httpHandler);

            var channel = new Channel()
            {
                HttpClient = new HttpClient(httpHandler),
                CloudBlobStorage = cloudBlobStorage,
                CryptoServices = cryptoProvider,
                Endpoint = sender,
                Logger = this.logger,
            };

            await channel.PostAsync(Valid.Message, new[] { receiver }, Valid.ExpirationUtc);
        }

        private async Task<IReadOnlyCollection<Channel.PayloadReceipt>> ReceiveMessageAsync(Mocks.CloudBlobStorageProviderMock cloudBlobStorage, Mocks.InboxHttpHandlerMock inboxMock, CryptoSettings cryptoProvider, OwnEndpoint receiver, bool expectMessage = true)
        {
            Requires.NotNull(cloudBlobStorage, "cloudBlobStorage");
            Requires.NotNull(receiver, "receiver");

            var httpHandler = new Mocks.HttpMessageHandlerMock();

            cloudBlobStorage.AddHttpHandler(httpHandler);
            inboxMock.Register(httpHandler);

            var channel = new Channel
            {
                HttpClient = new HttpClient(httpHandler),
                HttpClientLongPoll = new HttpClient(httpHandler),
                CloudBlobStorage = cloudBlobStorage,
                CryptoServices = cryptoProvider,
                Endpoint = receiver,
                Logger = this.logger,
            };

            var progressMessage = new TaskCompletionSource<Payload>();
            var progress = new Progress<Channel.PayloadReceipt>(m => progressMessage.SetResult(m.Payload));

            var messages = await channel.ReceiveAsync(progress: progress);
            if (expectMessage)
            {
                Assert.Equal(1, messages.Count);
                await progressMessage.Task;
                Assert.Same(progressMessage.Task.Result, messages.Single().Payload);
            }
            else
            {
                Assert.Equal(0, messages.Count);
            }

            return messages;
        }
    }
}
