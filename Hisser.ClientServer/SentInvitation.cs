/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents an Invitation sent to another user.
    /// </summary>
    public class SentInvitation : Message, IHisserStreamable
    {
        /// <summary>
        /// Your address.
        /// </summary>
        public string SenderAddress { get; private set; }

        /// <summary>
        /// A Secret Component created by you for use in further communication.
        /// </summary>
        public SentComponent SenderComponent { get; private set; }

        /// <summary>
        /// The Contact this invitation should be sent to.
        /// </summary>
        public Contact Receiver { get; private set; }
        
        /// <summary>
        /// Creates an Invitation that is sent to Contact.
        /// </summary>
        /// <param name="senderAddress">The Hisser Address of the Sender of this invitation.</param>
        /// <param name="senderComponent">The Secret Component sent with this invitation to encrypt future communication with.</param>
        /// <param name="receiver">The receiving Contact of this message.</param>
        public SentInvitation(string senderAddress, SentComponent senderComponent, Contact receiver)
        {
            this.SenderAddress = senderAddress;
            this.SenderComponent = senderComponent;
            this.Receiver = receiver;
        }

        /// <summary>
        /// Serializes this Invitation and writes it to the given Stream.
        /// </summary>
        /// <param name="stream">The stream to which this Invitation is written.</param>
        public async Task ToStream(HisserStream stream)
        {
            byte[] publicKey = GetPublicKey(Receiver.MyAlias);
            byte[] signature = DetermineSignature(Encoding.UTF8.GetBytes(
                string.Format("{0}.{1}.{2}",
                    Contact.ParseUsername(SenderAddress),
                    Receiver.MyAlias,
                    SenderComponent.PublicValue.ToString())), Receiver.MyAlias);

            await stream.WriteString1(Receiver.Username);
            await stream.WriteString1(SenderAddress);
            await stream.WriteString1(Contact.ParseUsername(SenderAddress));
            await stream.WriteString1(Receiver.MyAlias);
            await stream.WriteBytes2(publicKey);
            await stream.WriteBytes1(SenderComponent.PublicValue);
            await stream.WriteBytes2(signature);
            stream.WriteInt4(0); //todo: groups
        }
    }
}
