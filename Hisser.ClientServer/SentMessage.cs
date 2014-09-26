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
    /// Represents a Message sent to a Contact.
    /// </summary>
    public class SentMessage : Message, IHisserStreamable
    {
        /// <summary>
        /// Current message format version for sending new messages.
        /// </summary>
        public const int CURRENT_MESSAGEFORMAT_VERSION = 1;

        /// <summary>
        /// The receiver of this message.
        /// </summary>
        public Contact Receiver { get; private set; }

        /// <summary>
        /// The Data sent with this message.
        /// </summary>
        public MessageData Data { get; private set; }

        /// <summary>
        /// The Secret used as a key to encrypt the Data sent with this message.
        /// </summary>
        public Secret Secret { get; protected set; }

        /// <summary>
        /// Creates a ChatMessage that can be sent to a Receiver
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="secret"></param>
        /// <param name="data"></param>
        public SentMessage(Contact receiver, Secret secret, MessageData data)
        {
            Receiver = receiver;
            Secret = secret;
            Data = data;
        }

        /// <summary>
        /// Serializes this ChatMessage and writes it to the given Stream.
        /// </summary>
        /// <param name="stream">The stream to which the ChatMessage is written.</param>
        public async Task ToStream(HisserStream stream)
        {
            /*
             * A Chat Message consists of three parts:
             * (1) Receiver alias
             * (2) Message part
             * (3) Signature
             */

            //Determine the Message part and write it to a buffer.
            stream.WriteInt1(CURRENT_MESSAGEFORMAT_VERSION);
            switch (CURRENT_MESSAGEFORMAT_VERSION)
            {
                case 1:
                    {
                        await stream.WriteString1(Receiver.Alias);
                        stream.WriteInt4(Secret.MyComponent.Serial);
                        stream.WriteInt4(Secret.OtherComponent.Serial);

                        //This buffer will hold the encrypted Data part within the Message part
                        MemoryStream databuffer = new MemoryStream();
                        HisserStream databufferstream = new HisserStream(databuffer);
                        byte[] iv_aes = await Data.ToStream(Secret, databufferstream);
                        byte[] messageData = databuffer.ToArray();

                        await stream.WriteBytes1(iv_aes);
                        await stream.WriteBytes4(messageData);
                        await stream.WriteBytes2(DetermineSignature(messageData, Receiver.MyAlias));

                        break;
                    }
                default:
                    {
                        throw new FunctionalException(string.Format("Unknown message protocol version: {0}. Please check if the Server has the same Hisser version as you.", CURRENT_MESSAGEFORMAT_VERSION));
                    }
            }
        }
    }
}
