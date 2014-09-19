using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using System.Windows.Markup;
using WP7Contrib.View.Controls.Extensions;
using System.Diagnostics;
using Hisser.ClientServer;

namespace Hisser.UI.Phone
{
    public partial class ConversationPage : PhoneApplicationPage
    {
        private readonly TimeSpan CHECKMESSAGE_INTERVAL_MAX = new TimeSpan(0, 0, 20);    //Check messages each 20 seconds
        private readonly TimeSpan CHECKMESSAGE_INTERVAL_MIN = new TimeSpan(0, 0, 2);     //Check messages each 2 seconds
        private readonly TimeSpan CHECKMESSAGE_INTERVAL_DECAY = new TimeSpan(0, 0, 1);

        private Client _client;
        private Contact _contact;
        private DispatcherTimer _timer;
        private string _address = "test@hisser.nl";

        private ObservableCollection<MessageData> Items = new ObservableCollection<MessageData>();
        private Storyboard scrollViewerStoryboard;
        private DoubleAnimation scrollViewerScrollToEndAnim;

        //#region VerticalOffset DP

        /// <summary>
        /// VerticalOffset, a private DP used to animate the scrollviewer
        /// </summary>
        //private DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
        //  typeof(double), typeof(MainPage), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        /*private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainPage app = d as MainPage;
            app.OnVerticalOffsetChanged(e);
        }*/

        //private void OnVerticalOffsetChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    ConversationScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        //}

        //#endregion

        // Constructor
        public ConversationPage()
        {
            InitializeComponent();

            scrollViewerScrollToEndAnim = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(scrollViewerScrollToEndAnim, this);
            //Storyboard.SetTargetProperty(scrollViewerScrollToEndAnim, new PropertyPath(VerticalOffsetProperty));

            scrollViewerStoryboard = new Storyboard();
            scrollViewerStoryboard.Children.Add(scrollViewerScrollToEndAnim);
            this.Resources.Add("foo", scrollViewerStoryboard);
        }

        private void InitializeMessages()
        {
            Items = new ObservableCollection<MessageData>();

            foreach (MessageData md in _contact.ReadMessages(1000))
            {
                Items.Add(md);
            }

            ContentPanel.DataContext = Items;
        }

        async void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            try
            {
                int nrOfMessages = await _client.CheckMessages();

                if (nrOfMessages == 0 && _timer.Interval < CHECKMESSAGE_INTERVAL_MAX)
                {
                    _timer.Interval += CHECKMESSAGE_INTERVAL_DECAY;
                }
                else
                {
                    _timer.Interval = CHECKMESSAGE_INTERVAL_MIN;
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }

            _timer.Start();
        }

        void _client_OnExceptionOccured(object sender, ExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        private void ShowException(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            // loose focus to hide keyboard
            this.Focus();

            _client.
            
            Items.Add(new Message()
            {
                Side = MessageSide.Me,
                Text = TextInput.Text
            });

            string response = chatbot.GetResponse(TextInput.Text);
            TextInput.Text = "";

            Items.Add(new Message()
            {
                Side = MessageSide.You,
                Text = response
            });
        }

        private void TextInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ConversationContentContainer.ActualHeight < ConversationScrollViewer.ActualHeight)
            {
                PaddingRectangle.Show(() => ScrollConvesationToEnd());
            }
            else
            {
                ScrollConvesationToEnd();
            }

            ApplicationBar.IsVisible = true;
        }

        private void ScrollConvesationToEnd()
        {
            scrollViewerScrollToEndAnim.From = ConversationScrollViewer.VerticalOffset;
            scrollViewerScrollToEndAnim.To = ConversationContentContainer.ActualHeight;
            scrollViewerStoryboard.Begin();
        }

        private void TextInput_LostFocus(object sender, RoutedEventArgs e)
        {
            PaddingRectangle.Hide();
            ApplicationBar.IsVisible = false;
            ScrollConvesationToEnd();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("contact"))
            {
                string dc_connection_string = string.Format("{0}{1}.sdf", "Data Source=isostore:/", "test@hisser.nl");

                _client = new Client(
                    new ContactManager(dc_connection_string),
                    new NetworkCredential("test", "test"),
                    "hisser.nl",
                    new Device("test"));

                string contactAddress = NavigationContext.QueryString["contact"];
                _contact = _client.GetContacts().Where(x => x.Address == contactAddress).SingleOrDefault();

                if (_contact == null)
                {
                    throw new Exception(string.Format("Unknown contact: {0}.", contactAddress));
                }
                
                InitializeMessages();

                _client.OnExceptionOccured += _client_OnExceptionOccured;

                _timer = new DispatcherTimer();
                _timer.Tick += _timer_Tick;
                _timer.Interval = new TimeSpan(0, 0, 1);
                _timer.Start();

                this.DataContext = Items;
            }

            base.OnNavigatedTo(e);
        }
    }
}