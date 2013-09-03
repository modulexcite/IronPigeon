﻿namespace IronPigeon.Tests.Providers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using IronPigeon.Providers;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class DesktopCryptoProviderTests : CryptoProviderTests {
		private DesktopCryptoProvider provider;

		protected override ICryptoProvider CryptoProvider {
			get { return this.provider; }
		}

		[TestInitialize]
		public void Setup() {
			this.provider = new DesktopCryptoProvider();
		}

		[TestMethod]
		public void HashAlgorithmName() {
			Assert.AreEqual("SHA256", this.provider.SymmetricHashAlgorithmName); // default
			Assert.AreEqual("SHA1", this.provider.AsymmetricHashAlgorithmName); // default
			this.provider.SymmetricHashAlgorithmName = "SHA111";
			this.provider.AsymmetricHashAlgorithmName = "SHA222";
			Assert.AreEqual("SHA111", this.provider.SymmetricHashAlgorithmName);
			Assert.AreEqual("SHA222", this.provider.AsymmetricHashAlgorithmName);
		}
	}
}
