/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
//using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Threading;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents the Data-part of a Chat Message.
    /// </summary>
    [Table]
    public class MessageData
    {
        /// <summary>
        /// Current dataformat version for sending new messages.
        /// </summary>
        public const int CURRENT_DATAFORMAT_VERSION = 1;

        /// <summary>
        /// Size of the AES key used to encrypt this message.
        /// </summary>
        private const int AES_KEY_SIZE = 256;

        /// <summary>
        /// Database identifier of this Message Data.
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        internal long ID { get; private set; }

        /// <summary>
        /// ID of the Contact that this message was sent to or received from.
        /// </summary>
        [Column]
        internal long ContactID { get; private set; }

        /// <summary>
        /// Indicates according to which data format version this messsage data was created.
        /// </summary>
        [Column]
        internal int Dataformat_version { get; private set; }

        /// <summary>
        /// The Type of Content in this Message Data.
        /// </summary>
        [Column]
        public ContentType Type { get; private set; }

        /// <summary>
        /// The Content stored in this Message Data.
        /// </summary>
        [Column(DbType = "image")]
        public byte[] Content { get; private set; }

        /// <summary>
        /// The moment when this Message Data was created (by the Sender) or Received (by the Receiver).
        /// </summary>
        [Column]
        public DateTime Time { get; private set; }

        /// <summary>
        /// Indicates whether this message was Sent (true) or Received (false).
        /// </summary>
        public bool Sent { get; private set; }

        /// <summary>
        /// The Secret Component that is sent with this message.
        /// </summary>
        internal SecretComponent SecretComponent { get; private set; }

        /// <summary>
        /// Creates Message Data that can be sent to a receiving Contact.
        /// </summary>
        /// <param name="content">The content of the message.</param>
        /// <param name="contentType">The type of content.</param>
        /// <param name="sentComponent">The Secret Component created by the you that can be used in future communication.</param>
        /// <param name="contact">The Contact you are sending this message to.</param>
        public MessageData(
            object content, 
            ContentType contentType, 
            SentComponent sentComponent, 
            Contact contact)
        {
            this.ContactID = contact.ID;
            this.Type = contentType;
            this.Content = ToBytes(contentType, content);
            this.SecretComponent = sentComponent;
            this.Dataformat_version = CURRENT_DATAFORMAT_VERSION;
            this.Time = DateTime.Now;
            this.Sent = true;
        }

        /// <summary>
        /// Creates Message Data received from a sending Contact.
        /// </summary>
        /// <param name="content">The content of the message.</param>
        /// <param name="contentType">The type of content.</param>
        /// <param name="receivedComponent">The Secret Component created by the Sender that can be used in future communication.</param>
        /// <param name="dataformatVersion">The dataformat version used by the sender to create this message.</param>
        /// <param name="contact">The Contact that sent you this message.</param>
        private MessageData(
            byte[] content,
            ContentType contentType, 
            ReceivedComponent receivedComponent, 
            int dataformatVersion, 
            Contact contact)
        {
            this.ContactID = contact.ID;
            this.Type = contentType;
            this.Content = content;
            this.SecretComponent = receivedComponent;
            this.Dataformat_version = dataformatVersion;
            this.Time = DateTime.Now;
            this.Sent = false;
        }

        private MessageData()
        {
        }

        /// <summary>
        /// Gets the content of this Message Data as an Image.
        /// </summary>
        /// <returns></returns>
        public BitmapImage GetImage()
        {
            BitmapImage result = new BitmapImage();
            result.SetSource(new MemoryStream(Content));
            return result;
        }

        /// <summary>
        /// Gets the content of this Message Data as a String.
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return Encoding.UTF8.GetString(Content, 0, Content.Length);
        }

        /// <summary>
        /// Encrypts and serializes this message data to a Hisser Stream.
        /// </summary>
        /// <param name="iv_aes">The AES initialization vector to be used for encryption.</param>
        /// <param name="secret">The Diffie-Hellman Shared Secret to be used as key for encryption.</param>
        /// <param name="stream">The stream to write the serialized, encrypted data to.</param>
        public void ToStream(out byte[] iv_aes, Secret secret, HisserStream stream)
        {
            MemoryStream buffer = new MemoryStream();
            HisserStream messagebuffer = new HisserStream(buffer);
            messagebuffer.WriteInt1(CURRENT_DATAFORMAT_VERSION);
            messagebuffer.WriteBytes1(SecretComponent.PublicValue.ToByteArray());
            messagebuffer.WriteInt4(SecretComponent.Serial);
            messagebuffer.WriteInt4(0);
            messagebuffer.WriteString1(ContentType_ToString(Type));
            messagebuffer.WriteBytes4(Content);
            stream.WriteBytes4(Encrypt(out iv_aes, secret, buffer.ToArray()));
        }

        /// <summary>
        /// Deserializes and decrypts message data from a Hisser stream.
        /// </summary>
        /// <param name="iv_aes">The AES initialization vector that was used for encryption.</param>
        /// <param name="secret">The Diffie-Hellman Shared Secret that was used as key for encryption.</param>
        /// <param name="messagestream">The stream to read serialized, encrypted data from.</param>
        /// <param name="sender">The Contact that sent you this message.</param>
        /// <returns></returns>
        public static MessageData FromStream(byte[] iv_aes, Secret secret, HisserStream messagestream, Contact sender)
        {
            byte[] encryptedMessage = messagestream.ReadBytes4();
            byte[] decryptedMessage = Decrypt(iv_aes, secret, encryptedMessage);

            using (HisserStream stream = new HisserStream(new MemoryStream(decryptedMessage)))
            {
                int dataformat_version = stream.ReadInt1();

                //Receive a new SecretComponent
                byte[] new_dh_value = stream.ReadBytes1();
                long serial_new_dh_value = stream.ReadInt4();
                ReceivedComponent newComponent = new ReceivedComponent(serial_new_dh_value, new_dh_value);
                
                long group_id = stream.ReadInt4();
                ContentType type = ParseContentType(stream.ReadString1());
                byte[] contentBytes = stream.ReadBytes4();

                return new MessageData(contentBytes, type, newComponent, dataformat_version, sender);
            }
        }

        /// <summary>
        /// Decrypts Message Data.
        /// </summary>
        /// <param name="iv_aes">The AES initialization vector that was used for encryption.</param>
        /// <param name="secret">The Diffie-Hellman Shared Secret that was used as key for encryption.</param>
        /// <param name="encryptedContent">The content to be decrypted.</param>
        /// <returns>The decrypted content.</returns>
        public static byte[] Decrypt(byte[] iv_aes, Secret secret, byte[] encryptedContent)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                byte[] key = new byte[AES_KEY_SIZE];
                secret.Value.ToByteArray().CopyTo(key, 0);
                long requiredEncryptedContentLength = (encryptedContent.Length / aes.BlockSize + 1) * aes.BlockSize;
                aes.Padding = PaddingMode.Zeros;
                byte[] resizedEncryptedContent = new byte[requiredEncryptedContentLength];
                encryptedContent.CopyTo(resizedEncryptedContent, 0);

                using (MemoryStream result = new MemoryStream())
                {
                    using (MemoryStream ciphertext = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateDecryptor(key, iv_aes), CryptoStreamMode.Write))
                        {
                            cs.Write(resizedEncryptedContent, 0, resizedEncryptedContent.Length);
                            cs.Close();
                            return ciphertext.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts Message Data.
        /// </summary>
        /// <param name="iv_aes">The AES initialization vector to be used for encryption.</param>
        /// <param name="secret">The Diffie-Hellman Shared Secret to be used as key for encryption.</param>
        /// <param name="decryptedContent">The content to be encrypted.</param>
        /// <returns>The decrypted content.</returns>
        public static byte[] Encrypt(out byte[] iv_aes, Secret secret, byte[] decryptedContent)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                byte[] key = new byte[AES_KEY_SIZE];
                secret.Value.ToByteArray().CopyTo(key, 0);
                aes.Padding = PaddingMode.Zeros;
                iv_aes = RandomGenerator.GetBytes(16);

                using (MemoryStream ciphertext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(key, iv_aes), CryptoStreamMode.Write))
                    {
                        byte[] plaintextMessage = decryptedContent;
                        cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                        return ciphertext.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Converts the content represented by a given object to a byte array.
        /// </summary>
        /// <param name="type">The type of object.</param>
        /// <param name="content">The object to be converted.</param>
        /// <returns>The object represented as a byte array.</returns>
        private static byte[] ToBytes(ContentType type, object content)
        {
            switch (type)
            {
                case ContentType.TEXT_PLAIN:
                    {
                        return Encoding.ASCII.GetBytes((string)content);
                    }
                case ContentType.IMAGE_JPEG:
                    {
                        MemoryStream ms = new MemoryStream();
                        ((Image)content).Save(ms, ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                case ContentType.IMAGE_GIF:
                    {
                        MemoryStream ms = new MemoryStream();
                        ((Image)content).Save(ms, ImageFormat.Gif);
                        return ms.ToArray();
                    }
                case ContentType.IMAGE_PNG:
                    {
                        MemoryStream ms = new MemoryStream();
                        ((Image)content).Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                default:
                    {
                        throw new UnknownContentTypeException(type);
                    }
            }
        }

        public static string ContentType_ToString(ContentType type)
        {
            switch (type)
            {
                case ContentType.TEXT_PLAIN: return "text/plain";
                case ContentType.IMAGE_JPEG: return "image/jpeg";
                case ContentType.IMAGE_GIF: return "image/gif";
                case ContentType.IMAGE_PNG: return "image/png";
                default: throw new UnknownContentTypeException(type);

            }
        }

        public static ContentType ParseContentType(string type)
        {
            switch (type)
            {
                case "text/plain": return ContentType.TEXT_PLAIN;
                case "image/jpeg": return ContentType.IMAGE_JPEG;
                case "image/gif": return ContentType.IMAGE_GIF;
                case "image/png": return ContentType.IMAGE_PNG;
                default: throw new UnknownContentTypeException(type);
            }
        }
    
    }
}
