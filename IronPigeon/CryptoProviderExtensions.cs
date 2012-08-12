﻿namespace IronPigeon {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Microsoft;

	/// <summary>
	/// Extension methods to the <see cref="ICryptoProvider"/> interface.
	/// </summary>
	public static class CryptoProviderExtensions {
		/// <summary>
		/// Creates a web safe base64 thumbprint of some buffer.
		/// </summary>
		/// <param name="cryptoProvider">The crypto provider.</param>
		/// <param name="buffer">The buffer.</param>
		/// <returns>A string representation of a hash of the <paramref name="buffer"/>.</returns>
		public static string CreateWebSafeBase64Thumbprint(this ICryptoProvider cryptoProvider, byte[] buffer) {
			Requires.NotNull(cryptoProvider, "cryptoProvider");
			Requires.NotNull(buffer, "buffer");

			var hash = cryptoProvider.Hash(buffer);
			return Utilities.ToBase64WebSafe(hash);
		}

		/// <summary>
		/// Generates a new receiving endpoint.
		/// </summary>
		/// <param name="cryptoProvider">The crypto provider.</param>
		/// <param name="messageReceivingEndpointBaseUrl">The URL of the message relay service to use for the new endpoint.</param>
		/// <returns>The newly generated endpoint.</returns>
		/// <remarks>
		/// Depending on the length of the keys set in the provider and the amount of buffered entropy in the operating system,
		/// this method can take an extended period (several seconds) to complete.
		/// </remarks>
		public static OwnEndpoint GenerateNewEndpoint(this ICryptoProvider cryptoProvider, Uri messageReceivingEndpointBaseUrl = null) {
			Requires.NotNull(cryptoProvider, "cryptoProvider");

			byte[] privateEncryptionKey, publicEncryptionKey;
			byte[] privateSigningKey, publicSigningKey;

			cryptoProvider.GenerateEncryptionKeyPair(out privateEncryptionKey, out publicEncryptionKey);
			cryptoProvider.GenerateSigningKeyPair(out privateSigningKey, out publicSigningKey);

			var contact = new Endpoint() {
				EncryptionKeyPublicMaterial = publicEncryptionKey,
				SigningKeyPublicMaterial = publicSigningKey,
			};

			if (messageReceivingEndpointBaseUrl != null) {
				contact.MessageReceivingEndpoint = new Uri(
					messageReceivingEndpointBaseUrl,
					cryptoProvider.CreateWebSafeBase64Thumbprint(contact.EncryptionKeyPublicMaterial));
			}

			var ownContact = new OwnEndpoint(contact, privateSigningKey, privateEncryptionKey);
			return ownContact;
		}
	}
}