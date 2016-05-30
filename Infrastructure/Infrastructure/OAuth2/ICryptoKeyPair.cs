﻿using System.Security.Cryptography;

namespace AFT.RegoV2.Infrastructure.OAuth2
{
    /// <summary>Provides access to the public and private keys used to create and verify OAuth2 tokens.</summary>
    public interface ICryptoKeyPair
    {
        /// <summary>The public key used to encrypt access tokens and verify signatures.</summary>
        RSACryptoServiceProvider PublicSigningKey { get; }

        /// <summary>The private to sign and decrypt access tokens.</summary>
        RSACryptoServiceProvider PrivateEncryptionKey { get; }
    }
}   
