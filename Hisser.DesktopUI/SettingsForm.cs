/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Security;
using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// Represents a Form that can be used to change Username and Password.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private const string PASSWORD_UNCHANGED = "(unchanged)";

        public SettingsForm()
        {
            InitializeComponent();

            _address = Properties.Settings.Default.address;
            _password = 
                Properties.Settings.Default.password == null || Properties.Settings.Default.password.Length == 0? 
                null : 
                SecureStringHandler.DecryptString(Properties.Settings.Default.password);

            addressTextBox.Text = _address == null ? "" : _address; ;
            passwordTextBox.Text = _password == null? "" : PASSWORD_UNCHANGED;
        }

        /// <summary>
        /// Indicates whether the user has Cancelled this dialog.
        /// </summary>
        public bool Cancelled { get; private set; }

        private string _address;

        private SecureString _password;

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!Contact.ValidateAddress(addressTextBox.Text))
            {
                MessageBox.Show(string.Format("Invalid username: {0}. Expected something like: username@www.hisser.eu", addressTextBox.Text), "Invalid Username", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Properties.Settings.Default.address = addressTextBox.Text;

                //Save the password only if it was changed.
                if (_password == null && passwordTextBox.Text.Length > 0 || 
                    _password != null && passwordTextBox.Text != PASSWORD_UNCHANGED)
                {
                    _password = new SecureString();
                    foreach (char c in passwordTextBox.Text.ToCharArray())
                    {
                        _password.AppendChar(c);
                    }

                    Properties.Settings.Default.password = SecureStringHandler.EncryptString(_password);
                }
                
                Properties.Settings.Default.Save();
                Cancelled = false;
                Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Cancelled = true;
            Close();
        }

        private void passwordTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            passwordTextBox.PasswordChar = '*';
        }
    }
}
