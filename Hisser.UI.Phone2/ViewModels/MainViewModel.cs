using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Hisser.UI.Phone.Resources;
using Hisser.ClientServer;

namespace Hisser.UI.Phone.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(Client client, System.Net.NetworkCredential credentials, string address)
        {
            // TODO: Complete member initialization
            this.client = client;
            this.credentials = credentials;
            this.address = address;
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ContactViewModel> Items { get; private set; }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }

        /// <summary>
        /// Sample property that returns a localized string
        /// </summary>
        public string LocalizedSampleProperty
        {
            get
            {
                return AppResources.SampleProperty;
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            this.Items = new ObservableCollection<ContactViewModel>();
            foreach (Contact c in client.GetContacts())
            {
                Items.Add(new ContactViewModel(c));
            }

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private Client client;
        private System.Net.NetworkCredential credentials;
        private string address;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}