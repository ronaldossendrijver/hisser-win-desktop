using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hisser.ClientServer
{
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
        /// Creates a ChatMessage read from a stream
        /// </summary>
        /// <param name="reader"></param>
        public static Message FromStream(MessageHeader header, HisserStream stream, ContactManager context)
        {
            string receiver = stream.ReadString1();
            List<Contact> c = context.Contacts.ToList();

            Contact sender = context.Contacts.SingleOrDefault(x => x.MyAlias == receiver);
            if (sender == null)
                throw new UnknownAliasException();
            
            byte[] message = stream.ReadBytes4();
            byte[] signature = stream.ReadBytes2();

            if (!VerifySignature(message, signature, sender.PublicKey))
            {
                throw new Exception("Chat Message has been signed with an incorrect signature");
            }

            HisserStream messagestream = new HisserStream(new MemoryStream(message));

            int protocol_version = messagestream.ReadInt1();

            switch (protocol_version)
            {
                case 1:
                    {
                        //Read the receiver alias again (redundant)
                        receiver = messagestream.ReadString1();

                        //Determine which SecretComponents have been used to encrypt this message
                        long serial_a = messagestream.ReadInt4();
                        long serial_b = messagestream.ReadInt4();
                        
                        SentComponent myComponent = sender.MyComponents.SingleOrDefault(x => x.Serial == serial_b);
                        if (myComponent == null) throw new Exception(string.Format("Contact {0} has sent you a ChatMessage encoded with a SecretComponent you never created", sender.Username));
                        ReceivedComponent otherComponent = sender.OtherComponents.SingleOrDefault(x => x.Serial == serial_a);
                        if (otherComponent == null) throw new Exception(string.Format("Contact {0} has sent you a ChatMessage encoded with a SecretComponent you never received", sender.Username));

                        //Decrypt the message data
                        byte[] iv_aes = messagestream.ReadBytes1();
                        Secret secret = new Secret(myComponent, otherComponent);
                        MessageData decryptedData = MessageData.FromStream(iv_aes, secret, messagestream, sender);

                        return new ReceivedMessage(header, decryptedData, sender, secret);
                    }
                default:
                    {
                        throw new Exception(string.Format("Unknown message protocol version: {0}", protocol_version));
                    }
            }
        }
    }
}
