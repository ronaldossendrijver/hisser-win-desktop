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
    /// Represents a Chat Message Balloon that shows an image.
    /// </summary>
    public class ImageBalloon : MessageBalloon
    {
        private const float IMAGE_WEIGHT = .5f;

        private Image c;
        private Point loc;

        public ImageBalloon(MessagePanel container, MessageData message, HorizontalAlignment alignment)
            : base(container, message)
        {
            c = ContentHandler.GetImage(message);
            int imgWidth = (int)(IMAGE_WEIGHT * Width);
            Height = imgWidth * c.Height / c.Width;
            c = c.GetThumbnailImage(imgWidth, Height, ThumbnailCallback, IntPtr.Zero); ;

            if (alignment == HorizontalAlignment.Left)
            {
                loc = new Point(0, 0);
            }
            else
            {
                loc = new Point(Width - imgWidth, 0);
            }

            Region = new Region(new RectangleF(loc.X, loc.Y, imgWidth, Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(c, loc);
        }

        private bool ThumbnailCallback()
        {
            return false;
        }
    }
}
