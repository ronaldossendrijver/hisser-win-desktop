/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Hisser.ClientServer
{
    [Table]
    public class Contact
    {
        /// <summary>
        /// The ID of this Contact.
        /// </summary>
        [Column(IsPrimaryKey=true, IsDbGenerated=true)]
        internal long ID { get; private set; }

        /// <summary>
        /// The Hisser Address of this Contact. Example: ronald@hisser.nl.
        /// </summary>
        [Column]
        public string Address { get; private set; }

        /// <summary>
        /// The Alias under which this Contact is known to you. Used to send messages.
        /// </summary>
        [Column]
        public string Alias { get; private set; }

        /// <summary>
        /// The string under which you are known by this Contact. Used for sending messages to you.
        /// </summary>
        [Column]
        public string MyAlias { get; private set; }

        /// <summary>
        /// This Contact's public key in XML format (should be PEM format).
        /// </summary>
        [Column]
        public byte[] PublicKey { get; private set; }
        
        /// <summary>
        /// The Secret Components you have offered to this contact. The last Secret Component in this list has not yet
        /// been confirmed by this Contact. It might be confirmed by a future message.
        /// </summary>
        [Association(OtherKey = "ContactID")]
        internal EntitySet<SentComponent> MyComponents { get; private set; }

        /// <summary>
        /// The Secret Components you have received from this contact.
        /// </summary>
        [Association(OtherKey = "ContactID")]
        internal EntitySet<ReceivedComponent> OtherComponents { get; private set; }
        
        /// <summary>
        /// All Messages that have been exchanged (both sent and received) with this Contact.
        /// </summary>
        [Association(OtherKey = "ContactID")]
        internal EntitySet<MessageData> Messages { get; private set; }

        private ContactStatus _status = ContactStatus.Unknown;
        /// <summary>
        /// The status of this contact. E.g., status "Friend" means you can send messages to this Contact.
        /// </summary>
        [Column(Storage="_status")]
        public ContactStatus Status
        {
            get
            {
                return _status;
            }

            private set
            {
                if (TransitionAllowed(value)) {
                    _status = value;
                }
                else
                {
                    throw new Exception(string.Format("Illegal status transition: {0}->{1}", Status, value));
                }
            }
        }

        /// <summary>
        /// The Username of this Contact, derived from his Address.
        /// </summary>
        public string Username
        {
            get
            {
                return ParseUsername(Address);
            }
        }

        /// <summary>
        /// Creates a contact that you know of.
        /// </summary>
        /// <param name="hisserAddress">The Hisser Address of this Contact.</param>
        public Contact(string hisserAddress)
            : this()
        {
            int separator = hisserAddress.IndexOf('@');
            if (!ValidateAddress(hisserAddress))
            {
                throw new ArgumentException(string.Format("Invalid user address: {0}. Expected: username@hisser-server, e.g.: joe@www.hisser.eu", hisserAddress));
            }

            this.Address = hisserAddress;
            this.Status = ContactStatus.Known;
        }
                
        internal Contact()
        {
            MyComponents = new EntitySet<SentComponent>();
            OtherComponents = new EntitySet<ReceivedComponent>();
            Messages = new EntitySet<MessageData>();
        }

        /// <summary>
        /// Gets the X most recent messages sent to or received from this Contact.
        /// </summary>
        /// <param name="nrOfMessages">The number of messages to return.</param>
        /// <returns>The X most recent messages</returns>
        public List<MessageData> GetMessages(int nrOfMessages)
        {
            return Messages.OrderByDescending(x => x.Time).Take(nrOfMessages).ToList();
        }

        /// <summary>
        /// Register that you have sent this Contact an invitation request.
        /// </summary>
        /// <param name="myAlias">The Alias under which this Contact can send messages to you.</param>
        /// <returns>A Secret Component that you should send to this Contact for further communication.</returns>
        public SentComponent Invite(string myAlias)
        {
            this.MyAlias = myAlias;
            //Remove any existing offered components, in case some exist (from an earlier invitation).
            MyComponents.Clear();
            Status = ContactStatus.Invited;
            return CreateComponent();
        }
        
        /// <summary>
        /// Register that you have accepted this Contact as your friend.
        /// </summary>
        /// <param name="myAlias">The Alias under which this Contact can send messages to you.</param>
        /// <returns>A Secret Component that you should send to this Contact for further communication.</returns>
        public SentComponent AcceptInvitation(string myAlias)
        {
            this.MyAlias = myAlias; 
            Status = ContactStatus.Friend;
            return CreateComponent();
        }

        /// <summary>
        /// Register that this Contact has accepted you (and note his Alias and PublicKey).
        /// </summary>
        /// <param name="alias">The Alias under which this Contact will be known to you when sending messages.</param>
        /// <param name="publicKey">The Public Key (in XML format, should be in PEM format-> TODO) this Contact sent you when accepting your invitation.</param>
        /// <param name="offeredComponent">The Secret DH Component this Contact has offered you for further communication.</param>
        /// <returns>A Secret Component that you should send to this Contact for further communication.</returns>
        public SentComponent InvitationAccepted(string alias, byte[] publicKey, ReceivedComponent offeredComponent)
        {
            this.Alias = alias;
            this.PublicKey = publicKey;
            Status = ContactStatus.Friend;
            offeredComponent.ContactID = ID;
            OtherComponents.Add(offeredComponent);
            return CreateComponent();
        }

        /// <summary>
        /// Register that you received an invitation from this Contact.
        /// </summary>
        /// <param name="alias">The Alias under which this Contact will be known to you when sending messages.</param>
        /// <param name="publicKey">The Public Key (in XML format, should be in PEM format-> TODO) this Contact sent you when accepting your invitation.</param>
        /// <param name="offeredComponent">The Secret DH Component this Contact has offered you for further communication.</param>
        public void InvitationReceived(string alias, byte[] publicKey, ReceivedComponent offeredComponent)
        {
            this.Alias = alias;
            this.PublicKey = publicKey;
            Status = ContactStatus.Wannabe;
            offeredComponent.ContactID = ID;
            OtherComponents.Add(offeredComponent);
        }

        /// <summary>
        /// Creates a Secret based on the most recently received SecretComponent and the most recently confirmed sent SecretComponent.
        /// </summary>
        /// <returns>A Secret that can be used to encrypt a message sent to this Contact.</returns>
        public Secret CreateSecret()
        {
            if (MyComponents.Count > 1 && OtherComponents.Count > 0)
            {
                //Normal situation: there is at least one Sent SecretComponent that has been confirmed, and one Received SecretComponent.
                return new Secret(MyComponents[MyComponents.Count-2], OtherComponents[OtherComponents.Count-1]);
            }
            else
            {
                if (MyComponents.Count == 1 && OtherComponents.Count == 1)
                {
                    //Starting situation, just after exchanging invitations: there is one Sent SecretComponent and one Received SecretComponent.
                    return new Secret(MyComponents[0], OtherComponents[0]);
                }
                else
                {
                    throw new TechnicalException("You can't create a Secret because you have no confirmed Sent Components and/or no Received Components");
                }
            }
        }

        /// <summary>
        /// Utility to verify that the given address is a valid Hisser address.
        /// </summary>
        /// <param name="address">The address to be checked.</param>
        /// <returns>True iff the given address is valid.</returns>
        public static bool ValidateAddress(string address)
        {
            return address.IndexOf('@') > 0;
        }

        /// <summary>
        /// Parses the Username from a Hisser address.
        /// </summary>
        /// <param name="address">A Hisser address.</param>
        /// <returns>The Username part of the given address.</returns>
        public static string ParseUsername(string address)
        {
            return address.Substring(0, address.IndexOf('@'));
        }

        /// <summary>
        /// Parses the Server's hostname from a Hisser address.
        /// </summary>
        /// <param name="address">A Hisser address.</param>
        /// <returns>The Server's hostname part of the given address.</returns>
        public static string ParseServer(string address)
        {
            int seperatorIndex = address.IndexOf('@');
            return address.Substring(seperatorIndex, address.Length - seperatorIndex);
        }

        /// <summary>
        /// Creates a new Secret Component that can be sent to a Contact.
        /// </summary>
        /// <returns></returns>
        internal SentComponent CreateComponent()
        {
            SentComponent result = new SentComponent(MyComponents.Count + 1);
            result.ContactID = ID;
            MyComponents.Add(result);
            return result;
        }

        private bool TransitionAllowed(ContactStatus newStatus) {

            switch (Status)
            {
                case (ContactStatus.Unknown):
                    {
                        return (newStatus == ContactStatus.Known);
                    }
                case (ContactStatus.Known):
                    {
                        return (newStatus == ContactStatus.Invited || newStatus == ContactStatus.Wannabe);
                    }
                case (ContactStatus.Wannabe):
                    {
                        return (newStatus == ContactStatus.Friend || newStatus == ContactStatus.Rejected);
                    }
                case (ContactStatus.Invited):
                    {
                        return (newStatus == ContactStatus.Friend);
                    }
            }
        
            return false;
        }
    }
}
