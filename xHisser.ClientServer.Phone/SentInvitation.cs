/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;

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
        public void ToStream(HisserStream stream)
        {
            byte[] publicKey = GetPublicKey(Receiver.MyAlias);
            byte[] signature = DetermineSignature(Encoding.UTF8.GetBytes(
                string.Format("{0}.{1}.{2}",
                    Contact.ParseUsername(SenderAddress),
                    Receiver.MyAlias,
                    SenderComponent.PublicValue.ToString())), Receiver.MyAlias);

            stream.WriteKeyValue("receiver", Receiver.Username);
            stream.WriteKeyValue("sender", SenderAddress);
            stream.WriteKeyValue("name", Contact.ParseUsername(SenderAddress));
            stream.WriteKeyValue("alias", Receiver.MyAlias);
            stream.WriteKeyValue("public_key", publicKey);
            stream.WriteKeyValue("dh_value", SenderComponent.PublicValue.ToByteArray());
            stream.WriteKeyValue("signature", signature);
            stream.WriteKeyValue("group_id", 0); //todo: groups
        }
    }
}
