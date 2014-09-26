using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Hisser.UI.Phone
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/MainPage.xaml?action=settings&address={0}&password={1}", addressTextBox.Text, passwordPasswordBox.Password), UriKind.Relative));
        }
    }
}