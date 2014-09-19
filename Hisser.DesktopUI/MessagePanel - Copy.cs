using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    public partial class MessagePanel : Panel
    {
        private const float VERTICAL_SPACE = 10;
        private const float TRIANGLE_SIZE = 8;
        private readonly Pen IMAGE_BORDER = new Pen(Color.White, 1);

        public Color BackColorSent;
        public Color BackColorReceived;
        public Color ForeColorSent;
        public Color ForeColorReceived;

        private float y;
        private List<MessageData> _shownMessages;
        private Dictionary<RectangleF, MessageData> _positions;
        private bool topReached = true;

        public event EventHandler<MessageDataEventArgs> OnMessageClicked;
        public event EventHandler<EventArgs> OnMoreMessagesRequested;

        public MessagePanel()
        {
            _shownMessages = new List<MessageData>();
            _positions = new Dictionary<RectangleF, MessageData>();
            y = 0;
            ForeColorSent = Color.Black;
            BackColorSent = Color.FromArgb(200, 255, 200);
            ForeColorReceived = Color.Black;
            BackColorReceived = Color.FromArgb(215, 215, 215);

            InitializeComponent();
            this.AutoScroll = true;
            this.DoubleBuffered = true;
            VScrollProperties p = new VScrollProperties(this);
        }

        public void SetMessages(List<MessageData> messages) {

            _shownMessages = messages;
            Refresh();
            //VerticalScroll.Value = VerticalScroll.Maximum;
            //PerformLayout();
        }

        public void AddMessage(MessageData message)
        {
            int currentY = (int)y;
            _shownMessages.Add(message);
            Refresh();
            VerticalScroll.Value = Math.Min(currentY, VerticalScroll.Maximum);
            //PerformLayout();
        }

        public void AddMessages(List<MessageData> messages)
        {
            int currentY = (int)y;
            _shownMessages.AddRange(messages);
            Refresh();
            VerticalScroll.Value = Math.Min(currentY, VerticalScroll.Maximum);
            //PerformLayout();
        }

        public void Draw(Graphics g)
        {
            _positions.Clear();
            g.Clear(BackColor);
            y = 5;
            //g.DrawRectangle(new Pen(b_sent, 10), Bounds);

            foreach (MessageData message in _shownMessages)
            {
                DrawMessage(g, message);
            }
        }
        private int i;
        private void DrawMessage(Graphics g, MessageData m)
        {
            System.Diagnostics.Debug.Print("drawing: " + i++);
            //width of chat balloons (80% of panel width - 2xTRIANGLE_SIZE - width of vertical scrollbar)
            float h;
            RectangleF area;
            bool received = (m.Status == MessageStatus.ReceivedAndRead || m.Status == MessageStatus.ReceivedAndUnread);
            Brush b_back = received? new SolidBrush(BackColorReceived) : new SolidBrush(BackColorSent);
            Brush b_front = received? new SolidBrush(ForeColorReceived) : new SolidBrush(ForeColorSent);
            Pen p_front = received ? new Pen(ForeColorReceived) : new Pen(ForeColorSent);
            Pen p_back = received ? new Pen(BackColorReceived) : new Pen(BackColorSent);
            Font f = Font;

            g.ResetTransform();
            g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);

            switch (m.Type)
            {
                case ContentType.IMAGE_JPEG:
                case ContentType.IMAGE_GIF:
                case ContentType.IMAGE_PNG:
                    {
                        float w = (.9f * Size.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - 2 * TRIANGLE_SIZE) / 4;
                        float x = received ? TRIANGLE_SIZE : Size.Width - w - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - TRIANGLE_SIZE;
                        Image c = ContentHandler.GetImage(m);
                        float newHeight = w * c.Height / c.Width;
                        c = c.GetThumbnailImage((int)w, (int)newHeight, ThumbnailCallback, IntPtr.Zero);;
                        h = c.Height;
                        area = new RectangleF(x, y, c.Width, h);
                        g.DrawImage(c, area);
                        g.DrawRectangle(p_back, Rectangle.Round(area));
                        break;
                    }
                default:
                case ContentType.TEXT_PLAIN:
                    {
                        float w = .9f * Size.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - 2 * TRIANGLE_SIZE;
                        float x = received ? TRIANGLE_SIZE : Size.Width - w - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - TRIANGLE_SIZE;
                        PointF[] triangle;
                        if (received)
                        {
                            triangle = new PointF[] { new PointF(x, y + TRIANGLE_SIZE), new PointF(x, y + 2 * TRIANGLE_SIZE), new PointF(x - TRIANGLE_SIZE / 2, y + TRIANGLE_SIZE * 1.5f) };
                        }
                        else
                        {
                            triangle = new PointF[] { new PointF(x + w, y + TRIANGLE_SIZE), new PointF(x + w, y + 2 * TRIANGLE_SIZE), new PointF(x + w + TRIANGLE_SIZE / 2, y + TRIANGLE_SIZE * 1.5f) };
                        }
                        g.FillPolygon(b_back, triangle);

                        string c = ContentHandler.GetString(m);
                        if (c == "") c = " ";
                        StringFormat sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
                        h = g.MeasureString(c, f, (int)w, sf).Height;
                        area = new RectangleF(x, y, w, h);
                        g.FillRectangle(b_back, area);
                        g.DrawString(c, f, b_front, area);
                        break;
                    }
            }

            _positions.Add(area, m);
            y += h + VERTICAL_SPACE;

            if (y > AutoScrollMinSize.Height)
            {
                this.AutoScrollMinSize = new Size(0, (int)y);
            }
        }

        public bool ThumbnailCallback()
        {
            return false;
        }


        private Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            //Invalidate();
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

        protected override void OnClick(EventArgs e)
        {
            if (e is MouseEventArgs)
            {
                if (OnMessageClicked != null)
                {
                    MouseEventArgs me = (MouseEventArgs)e;
                    MessageData clickedMessage = PinpointMessage(me.X - this.AutoScrollPosition.X, me.Y - this.AutoScrollPosition.Y);

                    if (clickedMessage != null)
                    {
                        OnMessageClicked(this, new MessageDataEventArgs(clickedMessage));
                    }
                }
            }
            else
            {
                base.OnClick(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (PinpointMessage(e.X - this.AutoScrollPosition.X, e.Y - this.AutoScrollPosition.Y) != null)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private MessageData PinpointMessage(float x, float y)
        {
            return (from position in _positions.Keys
                    where position.Contains(x, y)
                    select _positions[position]).SingleOrDefault();
        }
    }

    public class MessageDataEventArgs : EventArgs
    {
        public MessageData Message { get; private set; }

        public MessageDataEventArgs(MessageData message)
        {
            Message = message;
        }
    }
}
