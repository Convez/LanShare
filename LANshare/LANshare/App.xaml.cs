﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
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
            try
            {
                string p = Path.GetTempPath() + "\\LANShare";
                Directory.EnumerateFiles(p).ToList().ForEach(File.Delete);
            }catch(Exception ex)
            {
                //la prossima volta
            }
            Model.Configuration.LoadConfiguration();
            Model.Configuration.CurrentUser.SessionId = Model.User.GenerateSessionId();
            //Check se è attiva una sessione dell'udp advertiser/tcp listener (la parte del programma con la trayicon)
            bool alreadyRunning = Connection.TCP_Comunication.OtherInstanceRunning();
            //Z:" LANshare.exe.manifest LANshare.pdb LANshareShellExt.dll LANshare.exe.config 
       
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
                    NotificationWindow C = new NotificationWindow("An instance of LANshare is already running.");
                    C.ShowDialog();

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
