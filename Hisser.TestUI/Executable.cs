using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text;
using Hisser.DesktopUI;
using Hisser.ClientServer;

namespace Hisser.TestUI
{
    static class Executable
    {
        private static void DisplayByteArray(string id, byte[] array)
        {
            string toPrint = "";
            foreach (byte b in array)
            {
                toPrint += (toPrint.Length == 0 ? "" : ",") + ((int)b).ToString();
            }
            System.Diagnostics.Debug.Print(id + ":" + toPrint);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string databaseFolder = string.Format("{0}\\Hisser", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            if (!Directory.Exists(databaseFolder))
            {
                Directory.CreateDirectory(databaseFolder);
            }

            string address = "test@hisser.nl";

            using (StreamWriter sw = new StreamWriter(
                File.OpenWrite(string.Format("{0}\\{1}.log", databaseFolder, address))))
            {
                Log.Start(sw, Log.LOGLEVEL_INF);

                string dc_connection_string = string.Format("{0}\\{1}.sdf", databaseFolder, address);

                NetworkCredential credentials = new NetworkCredential("test", "test");

                ContactManager context = new ContactManager(dc_connection_string);
                Client client = new Client(context, credentials, Contact.ParseServer(address));
                MainForm main = new MainForm(client, address);

                Application.Run(main);
            }
            
        }

    }
}
