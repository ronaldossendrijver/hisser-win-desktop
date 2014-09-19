/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Media;
using System.Configuration;
using System.Drawing.Imaging;
using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// The Hisser Desktop Client Form.
    /// </summary>
    public partial class MainForm : Form
    {
        private const int DEFAULT_NR_OF_MESSAGES_TO_SHOW = 20;
        private const int CHECKMESSAGE_INTERVAL_MAX = 20000;    //Check messages each 20 seconds
        private const int CHECKMESSAGE_INTERVAL_MIN = 2000;     //Check messages each 2 seconds
        private const int CHECKMESSAGE_INTERVAL_DECAY = 1000;
        private const int SMALL_WIDTH = 284;
        private const int LARGE_WIDTH = 800;

        private Client _client;
        private string _address;
        private BindingList<Contact> _shownContacts;
        private int _nrOfShownMessages = DEFAULT_NR_OF_MESSAGES_TO_SHOW;
        private ToolTip errorTooltip;

        public bool Restarted { get; private set; }

        public delegate void RefreshMethod(object o);

        public MainForm(Client client, string address)
        {
            _client = client;
            _address = address;

            InitializeComponent();

            List<Contact> allContacts = _client.GetContacts();

            //CONTACTS
            _shownContacts = new BindingList<Contact>((
                from contact in allContacts
                where contact.Status == ContactStatus.Friend || contact.Status == ContactStatus.Wannabe || contact.Status == ContactStatus.Invited
                select contact).ToList());

            contactGridView.DataSource = _shownContacts;
            foreach (DataGridViewColumn col in contactGridView.Columns)
            {
                col.Visible = false;
            }
            contactGridView.Columns["Address"].Visible = true;
            contactGridView.Columns["Address"].FillWeight = 10;
            contactGridView.CellFormatting += contactGridView_CellFormatting;

            //MESSAGES
            DetermineShownMessages();

            //Register events
            _client.OnInvitationAccepted += _client_OnInvitationAccepted;
            _client.OnInvitationSent += _client_OnInvitationSent;
            _client.OnInvitationReceived += _client_OnInvitationReceived;
            _client.OnMessageSent += _client_OnMessageSent;
            _client.OnMessageReceived += _client_OnMessageReceived;
            _client.OnMessagesReceived += _client_OnMessagesReceived;
            _client.OnExceptionOccured += _client_OnExceptionOccured;

            messagePanel.OnMoreMessagesRequested += messagePanel_OnMoreMessagesRequested;
            messagePanel.OnMessageClicked += messagePanel_OnMessageClicked;

            checkMessageTimer.Interval = CHECKMESSAGE_INTERVAL_MIN;
            checkMessageTimer.Start();

            Text = _address;
            
            #if DEBUG
            _clearServerButton.Visible = true;
            #endif

            AddTooltips();
        }

        void messagePanel_OnMoreMessagesRequested(object sender, EventArgs e)
        {
            _nrOfShownMessages += DEFAULT_NR_OF_MESSAGES_TO_SHOW;
            DetermineShownMessages();
        }
        
        void messagePanel_OnMessageClicked(object sender, MessageDataEventArgs e)
        {
            Contact selected = SelectedContact;
            MessageData clickedMessage = _client.GetMessage(selected, e.MessageID);

            switch (clickedMessage.Type)
            {
                case ContentType.IMAGE_GIF:
                    {
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.AddExtension = true;
                        fd.CheckPathExists = true;
                        fd.OverwritePrompt = true;
                        fd.Filter = "Image Files (*.gif)|*.gif";
                        fd.FilterIndex = 1;
                        fd.DefaultExt = "gif";
                        DialogResult result = fd.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            Image image = ContentHandler.GetImage(clickedMessage);
                            image.Save(fd.FileName);
                        }

                        break;
                    }
                case ContentType.IMAGE_JPEG:
                    {
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.AddExtension = true;
                        fd.CheckPathExists = true;
                        fd.OverwritePrompt = true;
                        fd.Filter = "Image Files (*.jpg;*.jpeg)|*.jpg;*.jpeg";
                        fd.DefaultExt = "jpg";
                        fd.FilterIndex = 1;
                        DialogResult result = fd.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            Image image = ContentHandler.GetImage(clickedMessage);
                            image.Save(fd.FileName);
                        }

                        break;
                    }
                case ContentType.IMAGE_PNG:
                    {
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.AddExtension = true;
                        fd.CheckPathExists = true;
                        fd.OverwritePrompt = true;
                        fd.Filter = "Image Files (*.png)|*.png";
                        fd.DefaultExt = "png";
                        fd.FilterIndex = 1;
                        DialogResult result = fd.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            Image image = ContentHandler.GetImage(clickedMessage);
                            image.Save(fd.FileName);
                        }

                        break;
                    }
                case ContentType.TEXT_PLAIN:
                    {
                        string text = ContentHandler.GetString(clickedMessage);
                        if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
                        {
                            System.Diagnostics.Process.Start(text);
                        }
                        else
                        {
                            Clipboard.SetText(text);
                        }

                        break;
                    }
            }
        }

        void _client_OnInvitationAccepted(object sender, ContactEventArgs e)
        {
            RefreshMethod rm = AddFriend;
            this.Invoke(rm, e.Contact);
        }
        private void AddFriend(object o)
        {
            contactGridView.Refresh();
            contactGridView_SelectionChanged(null, null);
            ShowInformation(string.Format("{0} added to Friends list.", ((Contact)o).Address));
        }

        void contactGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            Contact toFormat = (Contact)contactGridView.Rows[e.RowIndex].DataBoundItem;

            if (toFormat.Status == ContactStatus.Wannabe || toFormat.Status == ContactStatus.Invited)
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
            }

            if (_client.GetUnreadMessageCount(toFormat) > 0)
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
            }
        }

        void _client_OnMessagesReceived(object sender, MultipleMessageEventArgs e)
        {
            RefreshMethod rm = AddMessages;
            this.Invoke(rm, e.MessagesByContact);
        }
        void _client_OnMessageReceived(object sender, SingleMessageEventArgs e)
        {
            RefreshMethod rm = AddMessage;
            this.Invoke(rm, new Tuple<Contact, MessageData>(e.Contact, e.Message));
        }
        void _client_OnMessageSent(object sender, SingleMessageEventArgs e)
        {
            RefreshMethod rm = AddMessage;
            this.Invoke(rm, new Tuple<Contact, MessageData>(e.Contact, e.Message));
        }
        private void AddMessages(object o)
        {
            Dictionary<Contact, List<MessageData>> messagesByContact = (Dictionary<Contact, List<MessageData>>)o;
            Contact _selectedContact = SelectedContact;
            if (_selectedContact != null)
            {
                if (messagesByContact.ContainsKey(_selectedContact))
                {
                    foreach (MessageData md in messagesByContact[_selectedContact])
                    {
                        messagePanel.AddMessage(md);
                    }
                }
            }
        }
        private void AddMessage(object o)
        {
            Tuple<Contact, MessageData> contactAndMessage = (Tuple<Contact, MessageData>)o;
            if (SelectedContact != null)
            {
                if (contactAndMessage.Item1 == SelectedContact)
                {
                    messagePanel.AddMessage((MessageData)contactAndMessage.Item2);
                }
            }

        }

        void _client_OnInvitationSent(object sender, ContactEventArgs e)
        {
            RefreshMethod rm = AddInvited;
            this.Invoke(rm, e.Contact);
        }
        private void AddInvited(object o)
        {
            _shownContacts.Add((Contact)o);
            ShowInformation(string.Format("You sent an invitation to {0}.", ((Contact)o).Address));
        }

        private void _client_OnInvitationReceived(object sender, ContactEventArgs e)
        {
            RefreshMethod rm = AddInvitation;
            this.Invoke(rm, e.Contact);
        }
        private void AddInvitation(object o)
        {
            _shownContacts.Add((Contact)o);
            ShowInformation(string.Format("You received an invitation from {0}.", ((Contact)o).Address));
        }

        void _client_OnExceptionOccured(object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                RefreshMethod rm = HandleException;
                this.Invoke(rm, e.Exception);
            }
        }
        private void HandleException(object o)
        {
            Exception exception = (Exception)o;

            if (exception is AuthenticationRequiredException)
            {
                MessageBox.Show(exception.Message, "Incorrect Account Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowSettings();
            }
            else if (exception is MethodNotAllowedException || exception is UnknownContentTypeException)
            {
                MessageBox.Show(exception.Message, "Version Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (exception is InternalServerErrorException)
            {
                MessageBox.Show(exception.Message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (exception is AccountExpiredException)
            {
                MessageBox.Show(exception.Message, "Account Expired", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowSettings();
            }
            else if (exception is UnknownRecieverException)
            {
                MessageBox.Show(exception.Message, "Unknown Contact", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ShowError(exception.Message);
            }

            Log.WriteError(this, exception.Message);
        }

        void contactGridView_SelectionChanged(object sender, EventArgs e)
        {
            Contact selectedContact = SelectedContact;

            bool isFriend = (SelectedContact != null && selectedContact.Status == ContactStatus.Friend);
            bool isWannabe = (SelectedContact != null && selectedContact.Status == ContactStatus.Wannabe);

            messagePanel.Visible = isFriend;
            _sendButton.Visible = isFriend;
            buttonAttachment.Visible = isFriend;
            _messageTextBox.Visible = isFriend;
            acceptButton.Visible = isWannabe;

            if (isFriend)
            {
                _nrOfShownMessages = DEFAULT_NR_OF_MESSAGES_TO_SHOW;
                DetermineShownMessages();
                _messageTextBox.Focus();
                this.Width = LARGE_WIDTH;
            }
            else
            {
                this.Width = SMALL_WIDTH;
            }
        }
        
        private async void inviteButton_Click(object sender, EventArgs e)
        {
            checkMessageTimer.Enabled = false;

            try
            {
                string receiverAddress = "username@server";
                if (InputBox.Show("Invite Someone!", "Please enter the user's Hisser Address name to invite...", ref receiverAddress) == DialogResult.OK)
                {
                    if (Contact.ValidateAddress(receiverAddress))
                    {
                        ShowInformation(string.Format("Inviting {0}...", receiverAddress));
                        await _client.SendInvitation(_address, receiverAddress);
                        ShowInformation(string.Format("Invitation sent.", receiverAddress));
                    }
                    else
                    {
                        ShowError(string.Format("Invalid Hisser Address: {0}. Expected something like: username@www.hisser.nl", receiverAddress));
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            checkMessageTimer.Enabled = true;
        }

        void settingsButton_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }
        
        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            checkMessageTimer.Stop();
            _client.OnInvitationReceived -= _client_OnInvitationReceived;
            _client.OnMessageReceived -= _client_OnMessageReceived;
            _client.OnMessagesReceived -= _client_OnMessagesReceived;
            _client.OnExceptionOccured -= _client_OnExceptionOccured;
            _client.OnInvitationAccepted -= _client_OnInvitationAccepted;
            _client.OnMessageSent -= _client_OnMessageSent;
        }

        async void _sendButton_Click(object sender, EventArgs e)
        {
            if (_messageTextBox.Text.Length > 0)
            {
                await SendMessage(_messageTextBox.Text, ContentType.TEXT_PLAIN);
                _messageTextBox.Text = "";
            }
        }
        
        private Contact SelectedContact
        {
            get
            {
                //if (contactGridView.SelectedRows.Count == 1)
                if (contactGridView.CurrentRow != null)
                {
                    return (Contact)contactGridView.CurrentRow.DataBoundItem;
                }
                else
                {
                    return null;
                }
            }
        }

        private void ShowSettings()
        {
            checkMessageTimer.Stop();

            SettingsForm settings = new SettingsForm();
            settings.ShowDialog();

            if (!settings.Cancelled)
            {
                Restarted = true;
                Close();
            }

            checkMessageTimer.Start();
        }

        private async Task SendMessage(object toSend, ContentType type)
        {
            try
            {
                Contact selectedContact = SelectedContact;

                if (selectedContact != null)
                {
                    ShowInformation(string.Format("Sending message to {0}...", selectedContact.Address));
                    await _client.SendMessage(selectedContact, ContentHandler.GetBytes(type, toSend), type);
                    ShowInformation(string.Format("Message sent.", selectedContact.Address));
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        async void acceptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Contact selectedInvitation = SelectedContact;
                if (selectedInvitation != null)
                {
                    ShowInformation(string.Format("Accepting invitation from {0}...", selectedInvitation.Address));
                    await _client.AcceptInvitation(selectedInvitation, _address);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        async void checkMessageTimer_Tick(object sender, EventArgs e)
        {
            checkMessageTimer.Enabled = false;

            try
            {
                int nrOfMessages = await _client.CheckMessages();

                if (nrOfMessages == 0 && checkMessageTimer.Interval < CHECKMESSAGE_INTERVAL_MAX)
                {
                    checkMessageTimer.Interval += CHECKMESSAGE_INTERVAL_DECAY;
                }
                else
                {
                    checkMessageTimer.Interval = CHECKMESSAGE_INTERVAL_MIN;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            checkMessageTimer.Enabled = true;
        }

        async void _messageTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && _messageTextBox.Text.Length > 0)
            {
                e.Handled = true;
                await SendMessage(_messageTextBox.Text, ContentType.TEXT_PLAIN);
                _messageTextBox.Text = "";
            }
        }

        async void _clearServerButton_Click(object sender, EventArgs e)
        {
            checkMessageTimer.Enabled = false;

            try
            {
                ShowInformation(string.Format("Deleting all messages..."));
                await _client.DeleteAllMessages();
                ShowInformation(string.Format("Messages deleted."));
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            checkMessageTimer.Enabled = true;
        }

        private void ShowInformation(string message)
        {
            Log.WriteInfo(this, message);
        }

        private void ShowError(string message)
        {
            errorTooltip.RemoveAll();
            errorPictureBox.Visible = true;
            errorTooltip.ToolTipIcon = ToolTipIcon.Error;
            errorTooltip.SetToolTip(errorPictureBox, message);
        }

        async void buttonAttachment_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Image Files (*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png";
            fd.FilterIndex = 1;
            fd.Multiselect = false;
            DialogResult result = fd.ShowDialog();

            if (result == DialogResult.OK)
            {
                Image image = Image.FromFile(fd.FileName);
                if (image.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    await SendMessage(image, ContentType.IMAGE_JPEG);
                }
                else if (image.RawFormat.Equals(ImageFormat.Gif))
                {
                    await SendMessage(image, ContentType.IMAGE_GIF);
                }
                else if (image.RawFormat.Equals(ImageFormat.Png))
                {
                    await SendMessage(image, ContentType.IMAGE_PNG);
                }
                else
                {
                    MessageBox.Show("This image format is not supported.", "Unsupported format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private async void acceptButton_Click(object sender, EventArgs e)
        {
            Contact selectedInvitation = SelectedContact;
            if (selectedInvitation != null && selectedInvitation.Status == ContactStatus.Wannabe)
            {
                ShowInformation(string.Format("Accepting invitation from {0}...", selectedInvitation.Address));
                try
                {
                    await _client.AcceptInvitation(selectedInvitation, _address);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        private void _quitButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        private void AddTooltips()
        {
            ToolTip tt;

            tt = new ToolTip();
            tt.IsBalloon = true;
            tt.SetToolTip(settingsButton, "Click here to change your address and password.");

            tt = new ToolTip();
            bool firstContact = (_shownContacts.Count == 0);
            tt.ShowAlways = firstContact;
            tt.IsBalloon = true;
            tt.SetToolTip(inviteButton, firstContact ? "Click here to invite your first contact!" : "Click here to invite a contact.");

            tt = new ToolTip();
            tt.ShowAlways = true;
            tt.IsBalloon = true;
            tt.SetToolTip(acceptButton, "Click here to add this contact to your friends list.");

            tt = new ToolTip();
            tt.IsBalloon = true;
            tt.SetToolTip(buttonAttachment, "Click here to send this contact an image.");

            tt = new ToolTip();
            tt.IsBalloon = true;
            tt.SetToolTip(_sendButton, "Click here to send this contact the message you typed.");

            tt = new ToolTip();
            tt.IsBalloon = true;
            tt.SetToolTip(_messageTextBox, "Type your message here.");

            errorTooltip = new ToolTip();
            errorTooltip.IsBalloon = true;
            errorTooltip.ShowAlways = true;
        }

        private void DetermineShownMessages()
        {
            List<MessageData> shownMessages;
            Contact selectedContact = SelectedContact;

            if (selectedContact == null)
            {
                shownMessages = new List<MessageData>();
            }
            else
            {
                shownMessages = _client.ReadMessages(selectedContact, _nrOfShownMessages).OrderBy(x => x.Time).ToList();
            }

            messagePanel.SetMessages(shownMessages);
        }
    }
}