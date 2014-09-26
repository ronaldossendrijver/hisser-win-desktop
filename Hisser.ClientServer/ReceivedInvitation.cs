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
    /// Represents an invitation received from another user.
    /// </summary>
    public class ReceivedInvitation : Message
    {
        public MessageHeader Header { get; private set; }
        public string SenderAddress { get; private set; }
        public string Senderstring { get; private set; }
        public byte[] PublicKey { get; private set; }
        public ReceivedComponent OfferedComponent { get; private set; }

        /// <summary>
        /// Creates an Invitation received from a Contact
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="senderstring"></param>
        private ReceivedInvitation(MessageHeader header, string senderAddress, string senderstring, byte[] publicKey, ReceivedComponent offeredComponent)
        {
            Header = header;
            SenderAddress = senderAddress;
            Senderstring = senderstring;
            PublicKey = publicKey;
            OfferedComponent = offeredComponent;
        }

        /// <summary>
        /// Parses a Received Invitation from a stream.
        /// </summary>
        /// <param name="header">This invitation's message header.</param>
        /// <param name="stream">The Stream to read this information from.</param>
        /// <returns></returns>
        public static async Task<ReceivedInvitation> FromStream(MessageHeader header, HisserStream stream)
        {
            string receiverName = await stream.ReadString1();
            string senderAddress = await stream.ReadString1();
            string sendingUserName = await stream.ReadString1();
            string senderAlias = await stream.ReadString1();

            byte[] sendingUserPublicKey = await stream.ReadBytes2();
            byte[] dh_value = await stream.ReadBytes1();
            ReceivedComponent receivedComponent = new ReceivedComponent(1, dh_value);
            byte[] signedData = Encoding.UTF8.GetBytes(string.Format("{0}.{1}.{2}.{3}",
                senderAddress,
                sendingUserName,
                senderAlias,
                receivedComponent.PublicValue.ToString()));
            byte[] signature = await stream.ReadBytes2();

            if (!VerifySignature(signedData, signature, sendingUserPublicKey))
            {
                throw new Exception("Invitation has been signed with an incorrect signature");
            }

            return new ReceivedInvitation(header, senderAddress, senderAlias, sendingUserPublicKey, receivedComponent);
        }
    }
}
