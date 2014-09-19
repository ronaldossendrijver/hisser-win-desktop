/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Security;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents the client side of a connection to a Hisser Server.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Length of generated aliases.
        /// </summary>
        public const int ALIAS_LENGTH = 30;

        private Server _server;
        private ContactManager _context;
        private HashSet<long> _ignoredMessages;

        /// <summary>
        /// Occurs when the Client receives one message.
        /// </summary>
        public event EventHandler<SingleMessageEventArgs> OnMessageReceived;
        
        /// <summary>
        /// Occurs when the Clients receives more than one message at the same time.
        /// </summary>
        public event EventHandler<MultipleMessageEventArgs> OnMessagesReceived; 

        /// <summary>
        /// Occurs when a message has been succesfully sent to the Hisser Server.
        /// </summary>
        public event EventHandler<SingleMessageEventArgs> OnMessageSent;
        
        /// <summary>
        /// Occurs when an Invitation Request has been received.
        /// </summary>
        public event EventHandler<ContactEventArgs> OnInvitationReceived;

        /// <summary>
        /// Occurs when you have succesfully accepted an Invitation Request.
        /// </summary>
        public event EventHandler<ContactEventArgs> OnInvitationAccepted;

        /// <summary>
        /// Occurs when you have succesfully sent an Invitation Request.
        /// </summary>
        public event EventHandler<ContactEventArgs> OnInvitationSent;

        /// <summary>
        /// Occurs when an exception has occured on the client or server side.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnExceptionOccured;

        /// <summary>
        /// Creates a Client that communicates with a Hisser Server
        /// </summary>
        /// <param name="context">The DataContext that contains persisted information.</param>
        /// <param name="credentials">The credentials that should be used to log on to the Hisser Server.</param>
        /// <param name="hostname">The host name (e.g. www.hisser.nl) of the Hisser Server.</param>
        public Client(ContactManager context, NetworkCredential credentials, string hostname)
        {
            this._context = context;
            this._server = new Server(hostname, credentials);

            //This set is used to store corrupt messages to prevent them from showing up all the time.
            _ignoredMessages = new HashSet<long>();
        }

        /// <summary>
        /// Gives a list of all known Contacts. No changes should be made to these Contacts, as they are
        /// not persisted in the database.
        /// </summary>
        /// <returns>All known Contacts (with any Contact Status).</returns>
        public List<Contact> GetContacts()
        {
            return _context.Contacts.ToList();
        }

        /// <summary>
        /// Deletes all unread messages directed to this user and device from the server.
        /// </summary>
        /// <returns>The number of deleted messages.</returns>
        public async Task<int> DeleteAllMessages()
        {
            List<MessageHeader> msgs = await _server.GetMessageHeaders();
            foreach (MessageHeader m in msgs)
            {
                await _server.DeleteMessage(m);
            }
            return msgs.Count;
        }

        public async Task SetNotification(Notification notification)
        {
            await _server.SetNotification(notification);
        }

        /// <summary>
        /// Accept an invitation from the given contact.
        /// </summary>
        /// <param name="contact">The Contact that you want to accept as a friend.</param>
        /// <param name="accepterAddress">Your Hisser address.</param>
        /// <returns>An awaitable task.</returns>
        public async Task AcceptInvitation(Contact contact, string accepterAddress)
        {
            if (contact.Status == ContactStatus.Wannabe)
            {
                string alias = await GenerateAlias();

                if (alias != null)
                {
                    using (var tran = _context.BeginTransaction())
                    {
                        SentComponent toOffer = contact.AcceptInvitation(alias);
                        SentInvitation toSend = new SentInvitation(accepterAddress, toOffer, contact);
                        await _server.SendInvitation(toSend);
                        tran.Commit();
                    }
                    
                    if (OnInvitationAccepted != null)
                    {
                        OnInvitationAccepted(this, new ContactEventArgs(contact));
                    }
                }
                else
                {
                    throw new TechnicalException("Unable to create a new Alias.");
                }
            }
        }

        /// <summary>
        /// Sends a message containing data of a specific type to a Friendly Contact.
        /// </summary>
        /// <param name="receiver">The intended received of the data.</param>
        /// <param name="data">The data.</param>
        /// <param name="type">The type of data.</param>
        /// <returns>The message that was sent.</returns>
        public async Task<SentMessage> SendMessage(Contact receiver, byte[] data, ContentType type) {

            //Do we know an Alias for this user?
            if (receiver.Status == ContactStatus.Friend)
            {
                MessageData messageData;
                SentMessage toSend;

                using (var tran = _context.BeginTransaction())
                {
                    Secret secretToEncryptWith = receiver.CreateSecret();
                    messageData = new MessageData(data, type, receiver.MyComponents.Last(), receiver);
                    toSend = new SentMessage(receiver, secretToEncryptWith, messageData);
                    receiver.Messages.Add(messageData);
                    await _server.SendMessage(toSend);
                    tran.Commit();
                }

                if (OnMessageSent != null)
                {
                    OnMessageSent(this, new SingleMessageEventArgs(receiver, messageData));
                }

                return toSend;
            }
            else
            {
                throw new TechnicalException(string.Format("Unable to send a ChatMessage to a Contact with status {0}", receiver.Status));
            }
        }

        /// <summary>
        /// Sends an Invitation Request to the given Hisser address.
        /// </summary>
        /// <param name="senderAddress">The Hisser address of the intended recipient.</param>
        /// <param name="receiverAddress">Your Hisser address.</param>
        /// <returns>The Invitation that was sent.</returns>
        public async Task<SentInvitation> SendInvitation(string senderAddress, string receiverAddress)
        {
            if (senderAddress != receiverAddress)
            {
                //Check whether this address belongs to an already existing Contact
                Contact receiver = _context.Contacts.SingleOrDefault(x => x.Address == receiverAddress);

                if (receiver == null || receiver.Status == ContactStatus.Known || receiver.Status == ContactStatus.Unfriendly)
                {
                    //This is a new Contact
                    if (receiver == null)
                    {
                        using (var tran = _context.BeginTransaction())
                        {
                            receiver = new Contact(receiverAddress);
                            _context.Contacts.InsertOnSubmit(receiver);
                            tran.Commit();
                        }
                    }

                    using (var tran = _context.BeginTransaction())
                    {
                        string alias = await GenerateAlias();
                        SentComponent componentToOffer = receiver.Invite(alias);
                        SentInvitation toSend = new SentInvitation(senderAddress, componentToOffer, receiver);
                        await _server.SendInvitation(toSend);
                        tran.Commit();

                        if (OnInvitationSent != null)
                        {
                            OnInvitationSent(this, new ContactEventArgs(receiver));
                        }

                        return toSend;
                    }
                }
                else
                {
                    if (receiver.Status == ContactStatus.Invited)
                    {
                        throw new FunctionalException(string.Format("You already invited {0}. Please wait for this user to accept the invitation.", receiver.Username));
                    }
                    else
                    {
                        throw new FunctionalException(string.Format("You cannot invite a Contact with status {0}", receiver.Status));
                    }
                }
            }
            else
            {
                throw new FunctionalException("You can't invite yourself.");
            }
        }


        /// <summary>
        /// Gets the X most recent messages sent to or received from this Contact and gives unread messages the status "read".
        /// </summary>
        /// <param name="nrOfMessages">The number of messages to return.</param>
        /// <returns>The X most recent messages</returns>
        public List<MessageData> ReadMessages(Contact contact, int nrOfMessages)
        {
            /*
            var returnedMessages = contact.Messages.OrderByDescending(x => x.Time).Take(nrOfMessages);

            using (var tran = _context.BeginTransaction())
            {
                foreach (MessageData md in returnedMessages.Where(x => x.Status == MessageStatus.ReceivedAndUnread))
                {
                    md.Status = MessageStatus.ReceivedAndRead;
                }

                tran.Commit();
            }*/

            return contact.Messages.OrderByDescending(x => x.Time).Take(nrOfMessages).ToList();
        }

        /// <summary>
        /// Gets the X most recent messages sent to or received from this Contact, without changing the message status.
        /// </summary>
        /// <param name="nrOfMessages">The number of messages to return.</param>
        /// <returns>The X most recent messages</returns>
        public List<MessageData> GetMessages(Contact contact, int nrOfMessages)
        {
            return contact.Messages.OrderByDescending(x => x.Time).Take(nrOfMessages).ToList();
        }

        public MessageData GetMessage(Contact contact, long messageID)
        {
            return contact.Messages.SingleOrDefault(x => x.ID == messageID);
        }

        public int GetUnreadMessageCount(Contact contact)
        {
            return contact.Messages.Count(x => x.Status == MessageStatus.ReceivedAndUnread);
        }

        /// <summary>
        /// Retrieves any messages on the server.
        /// </summary>
        /// <returns>The number of messages that were retrieved.</returns>
        public async Task<int> CheckMessages()
        {
            Log.WriteInfo(this, "Checking for messages...");

            int nrOfInvitations = 0;
            int nrOfMessages = 0;
            Exception reportedException = null;
            List<MessageHeader> unreadMessages = null;

            try
            {
                unreadMessages = await _server.GetMessageHeaders();
            }
            catch (Exception ex)
            {
                reportedException = ex;
            }

            if (reportedException == null)
            {
                //First, process invitations in backward order
                foreach (MessageHeader message_header in (
                    from um in unreadMessages
                    where um.Type == MessageType.InvitationRequest
                    select um).OrderByDescending(x => x.ID))
                {
                    if (!_ignoredMessages.Contains(message_header.ID))
                    {
                        reportedException = null;

                        try
                        {
                            await ProcessInvitation((ReceivedInvitation)await _server.GetMessage(message_header, _context));
                            nrOfInvitations++;
                        }
                        catch (InvalidMessageIdException)
                        {
                            //_ignoredMessages.Add(message_header.ID);
                        }
                        catch (Exception e)
                        {
                            reportedException = e;
                        }

                        if (reportedException != null)
                            ReportException(reportedException);
                    }
                }

                Dictionary<Contact, List<MessageData>> messagesByContact = new Dictionary<Contact, List<MessageData>>();

                //Second, process chat messages in forward order 
                foreach (MessageHeader message_header in (
                    from um in unreadMessages
                    where um.Type == MessageType.ChatMessage
                    select um).OrderBy(x => x.ID))
                {
                    reportedException = null;

                    try
                    {
                        if (!_ignoredMessages.Contains(message_header.ID))
                        {
                            ReceivedMessage message = await ProcessMessage(message_header, _context);

                            if (messagesByContact.ContainsKey(message.Sender))
                            {
                                messagesByContact[message.Sender].Add(message.Data);
                            }
                            else
                            {
                                List<MessageData> messages = new List<MessageData>();
                                messages.Add(message.Data);
                                messagesByContact.Add(message.Sender, messages);
                            }

                            nrOfMessages++;
                        }
                    }
                    catch (UnknownAliasException)
                    {
                        _ignoredMessages.Add(message_header.ID);
                    }
                    catch (InvalidMessageIdException)
                    {
                        _ignoredMessages.Add(message_header.ID);
                    }
                    catch (Exception ex)
                    {
                        reportedException = ex;
                    }

                    if (reportedException != null)
                        ReportException(reportedException);
                }

                reportedException = null;

                //Report received messages
                try
                {
                    if (OnMessagesReceived != null) { }
                    if (nrOfMessages > 1)
                    {
                        OnMessagesReceived(this, new MultipleMessageEventArgs(messagesByContact));
                    }
                    else if (nrOfMessages == 1)
                    {
                        Contact sender = messagesByContact.Keys.First();
                        OnMessageReceived(this, new SingleMessageEventArgs(sender, messagesByContact[sender].First()));
                    }
                }
                catch (Exception ex)
                {
                    reportedException = ex;
                }

                if (reportedException != null)
                    ReportException(reportedException);

                reportedException = null;
            }

            if (reportedException != null)
                ReportException(reportedException);

            Log.WriteInfo(this, string.Format("Messages checked. Received invitations: {0}, messages: {1}.", nrOfInvitations, nrOfMessages));

            return nrOfInvitations + nrOfMessages;
        }

        private async Task<ReceivedMessage> ProcessMessage(MessageHeader message_header, ContactManager _context)
        {
            ReceivedMessage message = (ReceivedMessage)await _server.GetMessage(message_header, _context);

            using (var tran = _context.BeginTransaction())
            {
                //Store the offered Secret Component, if it's new
                ReceivedComponent offeredComponent = (ReceivedComponent)message.Data.SecretComponent;
                if ((from comp in message.Sender.OtherComponents where comp.Serial == offeredComponent.Serial select comp).Count() == 0)
                {
                    offeredComponent.ContactID = message.Sender.ID;
                    message.Sender.OtherComponents.Add(offeredComponent);
                }

                //Create a new SentComponent if the last one has been confirmed
                if (message.Secret.MyComponent.Serial == message.Sender.MyComponents.Last().Serial)
                {
                    message.Sender.CreateComponent();
                }

                message.Sender.Messages.Add(message.Data);
                tran.Commit();
            }

            await _server.DeleteMessage(message_header);

            return message;
        }

        private async Task ProcessInvitation(ReceivedInvitation ir)
        {
            //Check whether this contact is already known. If not, add it to the list of contacts.
            Contact contact = null;
            foreach (Contact c in _context.Contacts)
            {
                if (c.EqualAddress(ir.SenderAddress))
                {
                    contact = c;
                    break;
                }
            }

            if (contact == null || contact.Status == ContactStatus.Rejected)
            {
                if (contact == null)
                {
                    using (var tran = _context.BeginTransaction())
                    {
                        //Add this contact to your contact list
                        contact = new Contact(ir.SenderAddress);
                        _context.Contacts.InsertOnSubmit(contact);
                        tran.Commit();
                    }
                }

                using (var tran = _context.BeginTransaction())
                {
                    contact.InvitationReceived(ir.Senderstring, ir.PublicKey, ir.OfferedComponent);
                    tran.Commit();
                }

                if (OnInvitationReceived != null)
                {
                    OnInvitationReceived(this, new ContactEventArgs(contact));
                }
            }
            else if (contact.Status == ContactStatus.Invited)
            {
                //This contact has accepted your invitation!
                using (var tran = _context.BeginTransaction())
                {
                    contact.InvitationAccepted(ir.Senderstring, ir.PublicKey, ir.OfferedComponent);
                    tran.Commit();
                }

                if (OnInvitationReceived != null)
                {
                    OnInvitationAccepted(this, new ContactEventArgs(contact));
                }
            }

            await _server.DeleteMessage(ir.Header);
        }

        private void ReportException(Exception e)
        {
            if (OnExceptionOccured != null)
            {
                OnExceptionOccured(this, new ExceptionEventArgs(e));
            }
        }

        /// <summary>
        /// Performs cleanup of unused aliases on the server and old components in the client.
        /// </summary>
        /// <returns></returns>
        public async Task PerformCleanup()
        {
            List<string> existingAliases = await _server.GetAliases();
            List<string> usedAliases =
                (from contact in _context.Contacts
                 select contact.MyAlias).ToList();

            foreach (string alias in existingAliases)
            {
                if (!usedAliases.Contains(alias))
                {
                    try
                    {
                        await _server.DeleteAlias(alias);
                    }
                    catch (Exception)
                    {
                        //Ignore this exception
                    }
                }
            }
        }

        private async Task<string> GenerateAlias()
        {
            int nrOfRetries = 0;
            while (nrOfRetries++ < 10)
            {
                try
                {
                    string alias = RandomGenerator.GetString(ALIAS_LENGTH, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                    await _server.CreateAlias(alias);
                    return alias;
                }
                catch (AliasAlreadyExistsException)
                {

                }
            }

            throw new FunctionalException("Unable to create a new alias. Please contact the owner of the Hisser Server.");
        }


    }

    public class SingleMessageEventArgs : EventArgs
    {
        public Contact Contact { get; set; }
        public MessageData Message { get; set; }

        public SingleMessageEventArgs(Contact contact, MessageData message)
        {
            this.Contact = contact;
            this.Message = message;
        }
    }

    public class MultipleMessageEventArgs : EventArgs
    {
        public Dictionary<Contact, List<MessageData>> MessagesByContact { get; set; }

        public MultipleMessageEventArgs(Dictionary<Contact, List<MessageData>> messagesByContact)
        {
            this.MessagesByContact = messagesByContact;
        }
    }

    public class ContactEventArgs : EventArgs
    {
        public Contact Contact { get; set; }

        public ContactEventArgs(Contact contact)
        {
            this.Contact = contact;
        }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public ExceptionEventArgs(Exception exception)
        {
            this.Exception = exception;
        }
    }
}
