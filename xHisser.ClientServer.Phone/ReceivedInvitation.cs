using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Hisser.ClientServer
{
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
       
        public static ReceivedInvitation FromStream(MessageHeader header, HisserStream stream)
        {
            string receiverName = stream.ReadString1();
            string senderAddress = stream.ReadString1();
            string sendingUserName = stream.ReadString1();
            string senderstring = stream.ReadString1();

            byte[] sendingUserPublicKey = stream.ReadBytes2();
            byte[] dh_value = stream.ReadBytes1();
            ReceivedComponent receivedComponent = new ReceivedComponent(1, dh_value);
            byte[] signedData = Encoding.UTF8.GetBytes(string.Format("{0}.{1}.{2}",
                Contact.ParseUsername(senderAddress),
                senderstring,
                receivedComponent.PublicValue.ToString()));
            byte[] signature = stream.ReadBytes2();

            if (!VerifySignature(signedData, signature, sendingUserPublicKey))
            {
                throw new Exception("Invitation has been signed with an incorrect signature");
            }

            return new ReceivedInvitation(header, senderAddress, senderstring, sendingUserPublicKey, receivedComponent);
        }
    }
}
