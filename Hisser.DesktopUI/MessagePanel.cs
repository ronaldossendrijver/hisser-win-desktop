/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// Represents a Panel on which MessageData can be displayed as CHat Message Balloons.
    /// </summary>
    public partial class MessagePanel : Panel
    {
        private const int VERTICAL_SPACE = 10;
        private int y;
        private bool topReached = false;

        public event EventHandler<MessageDataEventArgs> OnMessageClicked;
        public event EventHandler<EventArgs> OnMoreMessagesRequested;

        public MessagePanel()
        {
            DoubleBuffered = true;
            InitializeComponent();
            y = 0;
        }

        public void SetMessages(List<MessageData> messages) {

            Controls.Clear();
            y = 0;

            foreach (MessageData message in messages)
            {
                _AddMessage(message);
            }
        }

        public void AddMessage(MessageData message)
        {
            _AddMessage(message);
            VerticalScroll.Value = VerticalScroll.Maximum;
        }

        public void AddMessages(List<MessageData> messages)
        {
            int currentY = (int)y;

            foreach (MessageData md in messages)
            {
                _AddMessage(md);
            }

            VerticalScroll.Value = Math.Min(currentY, VerticalScroll.Maximum);
        }

        private void _AddMessage(MessageData message)
        {
            MessageBalloon newBalloon = null;

            switch (message.Type)
            {
                case ContentType.TEXT_PLAIN:
                    {
                        newBalloon = new TextBalloon(this, message, message.Status == MessageStatus.Sent? HorizontalAlignment.Right : HorizontalAlignment.Left);
                        break;
                    }
                case ContentType.IMAGE_JPEG:
                case ContentType.IMAGE_GIF:
                case ContentType.IMAGE_PNG:
                    {
                        newBalloon = new ImageBalloon(this, message, message.Status == MessageStatus.Sent ? HorizontalAlignment.Right : HorizontalAlignment.Left);
                        break;
                    }
            }

            if (newBalloon != null)
            {
                newBalloon.Location = new Point(0, y + AutoScrollPosition.Y);
                newBalloon.MouseClick += newBalloon_MouseClick;
                y += newBalloon.Height + VERTICAL_SPACE;
                Controls.Add(newBalloon);
            }
        }

        void newBalloon_MouseClick(object sender, MouseEventArgs e)
        {
            if (OnMessageClicked != null)
            {
                MessageBalloon b = (MessageBalloon)sender;
                OnMessageClicked(this, new MessageDataEventArgs(b.MessageID));
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            if (OnMoreMessagesRequested != null)
            {
                if (this.AutoScrollPosition.Y == 0)
                {
                    if (topReached)
                    {
                        OnMoreMessagesRequested(this, null);
                        topReached = false;
                    }
                    else
                    {
                        topReached = true;
                    }
                }
                else
                {
                    topReached = false;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            Focus();
        }
    }

    public class MessageDataEventArgs : EventArgs
    {
        public long MessageID { get; private set; }

        public MessageDataEventArgs(long messageID)
        {
            MessageID = messageID;
        }
    }
}
