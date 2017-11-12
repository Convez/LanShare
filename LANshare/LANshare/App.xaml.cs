using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
<<<<<<< HEAD
=======
using System.Security.Cryptography;
using System.Text;
>>>>>>> Enrico_TB
using System.Threading.Tasks;
using System.Windows;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
<<<<<<< HEAD
      //  private bool _is_users_window_open = false;
=======
>>>>>>> Enrico_TB
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Model.Configuration.LoadConfiguration();
<<<<<<< HEAD
            //Check se è attiva una sessione dell'udp advertiser/tcp listener (la parte del programma con la trayicon)
            bool alreadyRunning = false;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpInfo = properties.GetActiveTcpListeners();
            foreach (IPEndPoint ep in tcpInfo)
            {
                if (ep.Port == Model.Configuration.TcpPort)
                {
                    alreadyRunning = true;
                    break;
                }
            }

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                //Ho dei file come argomento

                
                if (!alreadyRunning)
                {
                    Process.Start(System.Windows.Forms.Application.ExecutablePath);
                }
                var userWindow = new ShowUsersWindow();
                userWindow.Show();
                //is_users_window_open = true;
            }
            else
            {
                //Non ho file come argomento
                if (!alreadyRunning)
                {
                    //Sono il primo, altrimenti c'è già una istanza dell'applicazione aperta, o comunque qualcuno sta occupando la porta tcp
                    var trayIconWindow = new TrayIconWindow();
                    trayIconWindow.Activate();
                }
            }
        }

       
       
=======
            Model.Configuration.CurrentUser.SessionId = Model.User.GenerateSessionId();
            //Check se è attiva una sessione dell'udp advertiser/tcp listener (la parte del programma con la trayicon)
            bool alreadyRunning = Connection.TCP_Comunication.OtherInstanceRunning();

            if (e.Args.Length > 0)
            {
                //Have files to send
                if (alreadyRunning)
                {
                    //There is another instance running
                    Connection.TCP_Comunication com = new Connection.TCP_Comunication();
                    com.SendFileList(e.Args);
                    Shutdown();
                }
                else
                {
                    TrayIconWindow trayIcon = new TrayIconWindow(e);
                    trayIcon.Activate();
                }
            }
            else
            {
                //don't have files to send
                if (alreadyRunning)
                {
                    //There is another instance running. Don't need another one
                    MessageBox.Show("LANshare is already running");
                    Shutdown();
                }
                else
                {
                    TrayIconWindow trayIcon = new TrayIconWindow();
                    trayIcon.Activate();
                }
            }
        }
>>>>>>> Enrico_TB
    }
}
