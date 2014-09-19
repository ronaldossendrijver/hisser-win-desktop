/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.Security.Cryptography;
 
namespace Hisser.ClientServer
{
    /// <summary>
    /// The abstract Base Class for Invitations and ChatMessages.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Determines the signature to sign the given content, using a Private Key you have stored for communicating
        /// with the Contact with the given Alias. The Public Key corresponding to the used Private Key has been provided
        /// to that Contact at the time you exchanged invitations.
        /// </summary>
        /// <param name="content">The content to be signed.</param>
        /// <param name="alias">The Alias of the Contact you wish to send the content to. This determines the Private Key that should be used.</param>
        /// <returns>The signature to sign the given content with.</returns>
        public byte[] DetermineSignature(byte[] content, string alias)
        {
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = string.Format("Hisser.{0}", alias);

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp))
            {
                using (SHA256 sha = new SHA256Managed())
                {
                    return rsa.SignData(content, sha);
                }
            }
        }

        /// <summary>
        /// Verifies whether the given signature is valid for the given data, based on the given public key.
        /// </summary>
        /// <param name="content">The content that has been signed.</param>
        /// <param name="signature">The signature to be validated.</param>
        /// <param name="publicKey">The public key that should be used to verify signature validity.</param>
        /// <returns>True iff the given signature is valid.</returns>
        public static bool VerifySignature(byte[] content, byte[] signature, byte[] publicKey)
        {
            bool result = false;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                using (SHA256 sha = new SHA256Managed())
                {
                    rsa.ImportParameters(PemConverter.FromPem(publicKey));
                    result = rsa.VerifyData(content, sha, signature);
                    rsa.PersistKeyInCsp = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the Public Key corresponding with the Private Key used to sign content sent to the given Alias.
        /// </summary>
        /// <param name="alias">The Alias of the Contact you will send this Public Key to.</param>
        /// <returns></returns>
        public static byte[] GetPublicKey(string alias)
        {
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = string.Format("Hisser.{0}", alias);

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp))
            {
                rsa.PersistKeyInCsp = true;
                return PemConverter.ToPem(rsa.ExportParameters(false));
            }
        }
    }
}
