/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a message received from a Contact.
    /// </summary>
    public class ReceivedMessage : Message
    {
        public MessageHeader Header { get; private set; }

        public MessageData Data { get; private set; }

        public Contact Sender { get; private set; }

        public Secret Secret { get; private set; }

        /// <summary>
        /// Creates a ChatMessage that you have received from a Contact
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="secret"></param>
        /// <param name="iv_aes"></param>
        /// <param name="dataformat_version"></param>
        /// <param name="encrypted_message_content"></param>
        /// <param name="signature"></param>
        private ReceivedMessage(MessageHeader header, MessageData data, Contact sender, Secret secret)
        {
            Header = header;
            Data = data;
            Sender = sender;
            Secret = secret;
        }

        /// <summary>
        /// Creates a ChatMessage read from a stream.
        /// </summary>
        /// <param name="header">This message's header.</param>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="context">The context that holds Contact information, used to find the Contact that sent you this message.</param>
        /// <returns></returns>
        public static async Task<Message> FromStream(MessageHeader header, HisserStream stream, ContactManager context)
        {
            int protocol_version = stream.ReadInt1();
            string receiver = await stream.ReadString1();
            List<Contact> c = context.Contacts.ToList();

            Contact sender = context.Contacts.SingleOrDefault(x => x.MyAlias == receiver);
            if (sender == null)
                throw new UnknownAliasException();
            
            switch (protocol_version)
            {
                case 1:
                    {
                        //Determine which SecretComponents have been used to encrypt this message
                        long serial_a = stream.ReadInt4();
                        long serial_b = stream.ReadInt4();
                        
                        SentComponent myComponent = sender.MyComponents.SingleOrDefault(x => x.Serial == serial_b);
                        if (myComponent == null) throw new Exception(string.Format("Contact {0} has sent you a ChatMessage encoded with a SecretComponent you never created", sender.Username));
                        ReceivedComponent otherComponent = sender.OtherComponents.SingleOrDefault(x => x.Serial == serial_a);
                        if (otherComponent == null) throw new Exception(string.Format("Contact {0} has sent you a ChatMessage encoded with a SecretComponent you never received", sender.Username));

                        //Decrypt the message data
                        byte[] iv_aes = await stream.ReadBytes1();
                        byte[] message = await stream.ReadBytes4();
                        byte[] signature = await stream.ReadBytes2();

                        if (!VerifySignature(message, signature, sender.PublicKey))
                        {
                            throw new Exception("Chat Message has been signed with an incorrect signature");
                        }

                        Secret secret = new Secret(myComponent, otherComponent);

                        using (MemoryStream messageData = new MemoryStream(message))
                        {
                            MessageData decryptedData = await MessageData.FromStream(iv_aes, secret, new HisserStream(messageData), sender);
                            return new ReceivedMessage(header, decryptedData, sender, secret);
                        }
                    }
                default:
                    {
                        throw new Exception(string.Format("Unknown message protocol version: {0}", protocol_version));
                    }
            }
        }
    }
}
