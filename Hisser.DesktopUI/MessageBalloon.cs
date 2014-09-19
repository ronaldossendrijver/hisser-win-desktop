/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// Represents a Chat Message Balloon.
    /// </summary>
    public abstract class MessageBalloon : Control
    {
        protected Brush b_back;
        protected Brush b_front;
        protected Pen p_front;
        protected Pen p_back;
        
        public long MessageID { get; private set; }

        public MessageBalloon(MessagePanel container, MessageData message)
        {
            this.MessageID = message.ID;

            bool sent = (message.Status == MessageStatus.Sent);

            if (sent)
            {
                b_back = new SolidBrush(Color.FromArgb(215, 215, 215));
                b_front = new SolidBrush(Color.Black);
                p_back = new Pen(Color.FromArgb(215, 215, 215));
                p_front = new Pen(Color.Black);
                
            }
            else
            {
                b_back = new SolidBrush(Color.FromArgb(215, 250, 215));
                b_front = new SolidBrush(Color.Black);
                p_back = new Pen(Color.FromArgb(215, 250, 215));
                p_front = new Pen(Color.Black);
            }

            Width = container.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
            Font = container.Font;
            Cursor = Cursors.Hand;
        }
    }
}
