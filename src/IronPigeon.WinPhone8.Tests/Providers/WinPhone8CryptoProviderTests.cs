﻿namespace IronPigeon.WinPhone8.Tests {
	using System;
	using System.Security.Cryptography;
	using IronPigeon.Tests;
	using IronPigeon.Providers;
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

	[TestClass]
	public class WinPhone8CryptoProviderTests : CryptoProviderTests {
		private WinPhone8CryptoProvider provider;

		protected override ICryptoProvider CryptoProvider {
			get { return this.provider; }
		}

		[TestInitialize]
		public void Setup() {
			this.provider = new WinPhone8CryptoProvider();
		}
	}
}
