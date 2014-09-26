/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security;
using System.Configuration;
using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// Represents the Executable that runs a Hisser Desktop Client.
    /// </summary>
    public static class Executable
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                /*
                 * The Main Form will be automatically restarted upon closing, except when the user itself closed the 
                 * form. Purpose of this is to restart the application upon a change of settings or fatal exception.
                 */
                bool shouldRun = true;
                while (shouldRun)
                {
                    bool userHasCancelled = false;
                    string address = Properties.Settings.Default.address;
                    string securepw = Properties.Settings.Default.password;

                    while (!ValidateSettings(address, securepw) && !userHasCancelled)
                    {
                        /*
                         * A Settings form will be shown time and time again until either the user canceled it
                         * or the user cancelled entering settings.
                         */
                        SettingsForm settings = new SettingsForm();
                        Application.Run(settings);
                        userHasCancelled = settings.Cancelled;

                        if (!userHasCancelled)
                        {
                            address = Properties.Settings.Default.address;
                            securepw = Properties.Settings.Default.password;
                        }
                    }

                    if (ValidateSettings(address, securepw))
                    {
                        //If valid settings exist, show the Main window.
                        string username = Contact.ParseUsername(address);
                        string hostname = Contact.ParseServer(address);
                        SecureString password = SecureStringHandler.DecryptString(securepw);
                        NetworkCredential credentials = new NetworkCredential(username, password);

                        //Define database and ContactManager
                        string databaseFolder = string.Format("{0}\\Hisser", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                        if (!Directory.Exists(databaseFolder))
                        {
                            Directory.CreateDirectory(databaseFolder);
                        }

                        using (StreamWriter sw = new StreamWriter(
                            File.OpenWrite(string.Format("{0}\\{1}.log", databaseFolder, address))))
                        {
                            #if DEBUG
                            Log.Start(sw, Log.LOGLEVEL_INF);
                            #else
                            Log.Start(sw, Log.LOGLEVEL_ERR);
                            #endif

                            string dc_connection_string = string.Format("{0}\\{1}.sdf", databaseFolder, address);
                            ContactManager context = new ContactManager(dc_connection_string);

                            //Create the Hisser client and show the Main form.
                            Client client = new Client(context, credentials, hostname);
                            MainForm main = new MainForm(client, address);

                            Application.Run(main);

                            //Restart only if the Main form indicates that it was restarted.
                            shouldRun = main.Restarted;

                            Log.Stop();
                        }
                    }
                    else
                    {
                        //Don't Restart if the user did not enter valid settings
                        shouldRun = false;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private static bool ValidateSettings(string address, string securepw) {
            return !(address == null || address.Length == 0 || securepw == null || securepw.Length == 0);
        }
    }
}
