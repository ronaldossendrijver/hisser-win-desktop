using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Hisser.ClientServer;

namespace Hisser.UI.Phone
{
    public partial class NewContactPage : PhoneApplicationPage
    {
        public NewContactPage()
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
            NavigationService.Navigate(new Uri(string.Format("/MainPage.xaml?action=contact&address={0}", addressTextBox.Text), UriKind.Relative));
        }
    }
}