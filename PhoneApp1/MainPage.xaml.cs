using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using Hisser.UI.Phone.Resources;
using Hisser.ClientServer;

namespace Hisser.UI.Phone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private readonly TimeSpan CHECKMESSAGE_INTERVAL_MAX = new TimeSpan(0, 0, 20);    //Check messages each 20 seconds
        private readonly TimeSpan CHECKMESSAGE_INTERVAL_MIN = new TimeSpan(0, 0, 2);     //Check messages each 2 seconds
        private readonly TimeSpan CHECKMESSAGE_INTERVAL_DECAY = new TimeSpan(0, 0, 1);

        public ObservableCollection<ContactView> Items { get; private set; }

        Client _client;
        private DispatcherTimer _timer;
        private string _address = "test@hisser.nl";

        // Constructor
        public MainPage()
        {
            string dc_connection_string = string.Format("{0}{1}.sdf", "Data Source=isostore:/", "test@hisser.nl");

            _client = new Client(
                new ContactManager(dc_connection_string), 
                new NetworkCredential("test", "test"), 
                "hisser.nl", 
                new Device("test"));

            InitializeComponent();
            InitializeContacts();

            _client.OnInvitationAccepted += _client_OnInvitationAccepted;
            _client.OnInvitationReceived += _client_OnInvitationReceived;
            _client.OnExceptionOccured += _client_OnExceptionOccured;

            _timer = new DispatcherTimer();
            _timer.Tick += _timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
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

        private void ShowException(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        void _client_OnInvitationReceived(object sender, ContactEventArgs e)
        {
            Items.Add(new ContactView(e.Contact));
        }

        void _client_OnExceptionOccured(object sender, ExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        void _client_OnInvitationAccepted(object sender, ContactEventArgs e)
        {
            Items.Add(new ContactView(e.Contact));
        }

        private void InitializeContacts()
        {
            Items = new ObservableCollection<ContactView>();

            foreach (Contact c in _client.GetContacts())
            {
                Items.Add(new ContactView(c));
            }

            ContentPanel.DataContext = Items;
        }


        private void hisserPhoneApplicationPage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(sender.ToString());
        }

        private void hisserPhoneApplicationPage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(sender.ToString());
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewContactPage.xaml", System.UriKind.Relative));
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", System.UriKind.Relative));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("action"))
            {
                string action = NavigationContext.QueryString["action"];

                switch (action)
                {
                    case "settings":
                        {
                            string address = NavigationContext.QueryString["address"];
                            string password = NavigationContext.QueryString["password"];
                            break;
                        }
                    case "contact":
                        {
                            string address = NavigationContext.QueryString["address"];
                            SentInvitation invitation = await _client.SendInvitation(_address, address);
                            var x = ContentPanel.DataContext;
                            int y = ContentPanel.RowDefinitions.Count;


                            if (invitation != null)
                            {
                                Items.Add(new ContactView(invitation.Receiver));
                            }

                            var p = ContentPanel.DataContext;
                            int q = ContentPanel.RowDefinitions.Count;


                            break;
                        }
                }
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}