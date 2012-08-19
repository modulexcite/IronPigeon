﻿namespace IronPigeon {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Microsoft;

	/// <summary>
	/// Describes common predefined security levels for communications.
	/// </summary>
	public abstract class SecurityLevel {
		/// <summary>
		/// The recommended security level for personal or business communications.
		/// Keys may take several seconds to generate, but provide maximum protection.
		/// </summary>
		public static readonly SecurityLevel Recommended = new MaximumLevel();

		/// <summary>
		/// The minimal security level that still includes encryption and signatures.
		/// Keys are generated much faster, but provide less protection.
		/// </summary>
		public static readonly SecurityLevel Minimal = new MinimalLevel();

		/// <summary>
		/// Gets the name of the hash algorithm.
		/// </summary>
		/// <value>
		/// The name of the hash algorithm.
		/// </value>
		public abstract string HashAlgorithmName { get; }

		/// <summary>
		/// Gets the size of the encryption asymmetric key.
		/// </summary>
		/// <value>
		/// The size of the encryption asymmetric key.
		/// </value>
		public abstract int EncryptionAsymmetricKeySize { get; }

		/// <summary>
		/// Gets the size of the signature asymmetric key.
		/// </summary>
		/// <value>
		/// The size of the signature asymmetric key.
		/// </value>
		public abstract int SignatureAsymmetricKeySize { get; }

		/// <summary>
		/// Gets the size of the BLOB symmetric key.
		/// </summary>
		/// <value>
		/// The size of the BLOB symmetric key.
		/// </value>
		public abstract int BlobSymmetricKeySize { get; }

		/// <summary>
		/// Sets this security level's key lengths to the specified crypto provider.
		/// </summary>
		/// <param name="cryptoProvider">The crypto provider.</param>
		public void Apply(ICryptoProvider cryptoProvider) {
			Requires.NotNull(cryptoProvider, "cryptoProvider");

			cryptoProvider.HashAlgorithmName = this.HashAlgorithmName;
			cryptoProvider.EncryptionAsymmetricKeySize = this.EncryptionAsymmetricKeySize;
			cryptoProvider.SignatureAsymmetricKeySize = this.SignatureAsymmetricKeySize;
			cryptoProvider.BlobSymmetricKeySize = this.BlobSymmetricKeySize;
		}

		/// <summary>
		/// Minimal key sizes.
		/// </summary>
		private class MinimalLevel : SecurityLevel {
			/// <summary>
			/// Gets the name of the hash algorithm.
			/// </summary>
			/// <value>
			/// The name of the hash algorithm.
			/// </value>
			public override string HashAlgorithmName {
				get { return "SHA1"; }
			}

			/// <summary>
			/// Gets the size of the encryption asymmetric key.
			/// </summary>
			/// <value>
			/// The size of the encryption asymmetric key.
			/// </value>
			public override int EncryptionAsymmetricKeySize {
				get { return 512; }
			}

			/// <summary>
			/// Gets the size of the signature asymmetric key.
			/// </summary>
			/// <value>
			/// The size of the signature asymmetric key.
			/// </value>
			public override int SignatureAsymmetricKeySize {
				get { return 512; }
			}

			/// <summary>
			/// Gets the size of the BLOB symmetric key.
			/// </summary>
			/// <value>
			/// The size of the BLOB symmetric key.
			/// </value>
			public override int BlobSymmetricKeySize {
				get { return 128; }
			}
		}

		/// <summary>
		/// Maximum recommended key length.s.
		/// </summary>
		private class MaximumLevel : SecurityLevel {
			/// <summary>
			/// Gets the name of the hash algorithm.
			/// </summary>
			/// <value>
			/// The name of the hash algorithm.
			/// </value>
			public override string HashAlgorithmName {
				get { return "SHA256"; }
			}

			/// <summary>
			/// Gets the size of the encryption asymmetric key.
			/// </summary>
			/// <value>
			/// The size of the encryption asymmetric key.
			/// </value>
			public override int EncryptionAsymmetricKeySize {
				get { return 4096; }
			}

			/// <summary>
			/// Gets the size of the signature asymmetric key.
			/// </summary>
			/// <value>
			/// The size of the signature asymmetric key.
			/// </value>
			public override int SignatureAsymmetricKeySize {
				get { return 4096; }
			}

			/// <summary>
			/// Gets the size of the BLOB symmetric key.
			/// </summary>
			/// <value>
			/// The size of the BLOB symmetric key.
			/// </value>
			public override int BlobSymmetricKeySize {
				get { return 256; }
			}
		}
	}
}
