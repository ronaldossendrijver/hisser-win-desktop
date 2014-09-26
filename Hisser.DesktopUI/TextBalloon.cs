/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// Represents a Chat Message Balloon that holds a piece of text.
    /// </summary>
    public class TextBalloon : MessageBalloon
    {
        readonly StringFormat SF = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        private const float TRIANGLE_SIZE = 4;
        private const float TEXTBOX_WEIGHT = .8f;

        private string c;
        private RectangleF area;
        private PointF[] triangle;

        public TextBalloon(MessagePanel container, MessageData message, HorizontalAlignment alignment)
            : base(container, message)
        {
            c = ContentHandler.GetString(message);
            if (c == "") c = " ";

            if (Uri.IsWellFormedUriString(c, UriKind.Absolute))
            {
                Font = new Font(Font, FontStyle.Underline);
                b_front = b_front = new SolidBrush(Color.Blue);
            }

            Height = (int)CreateGraphics().MeasureString(c, Font, Width, SF).Height;
            float w = TEXTBOX_WEIGHT * Width - 2f * TRIANGLE_SIZE;

            if (alignment == HorizontalAlignment.Left)
            {
                float x = TRIANGLE_SIZE;
                area = new RectangleF(x, 0, w, Height);
                triangle = new PointF[] { 
                    new PointF(x, TRIANGLE_SIZE), 
                    new PointF(x, 3f * TRIANGLE_SIZE), 
                    new PointF(0, 2f * TRIANGLE_SIZE) };
                Region = new Region(new RectangleF(0, 0, w, Height));
            }
            else 
            {
                float x = Width - w - TRIANGLE_SIZE;
                area = new RectangleF(x, 0, w, Height);
                triangle = new PointF[] { 
                    new PointF(Width - TRIANGLE_SIZE, TRIANGLE_SIZE), 
                    new PointF(Width - TRIANGLE_SIZE, 3f * TRIANGLE_SIZE), 
                    new PointF(Width, 2f * TRIANGLE_SIZE) };
                Region = new Region(new RectangleF(x, 0, TRIANGLE_SIZE + w, Height));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillPolygon(b_back, triangle);
            e.Graphics.FillRectangle(b_back, area);
            e.Graphics.DrawString(c, Font, b_front, area);
        }
    }
}
