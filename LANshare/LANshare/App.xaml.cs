using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Model.Configuration.LoadConfiguration();
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
    }
}
