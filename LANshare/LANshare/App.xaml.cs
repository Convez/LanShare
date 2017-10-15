using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
      //  private bool _is_users_window_open = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Model.Configuration.LoadConfiguration();
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

        //public bool is_users_window_open
        //{
        //    get{ return _is_users_window_open; }
        //    set { _is_users_window_open = value; }
        //}
       
    }
}
