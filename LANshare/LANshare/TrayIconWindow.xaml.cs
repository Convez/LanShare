using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LANshare.Connection;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for TrayIconWindow.xaml
    /// </summary>
    public partial class TrayIconWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _trayIcon;

        private LanComunication _comunication;
        private Task _UDPadvertiser;

        private TCP_FileTransfer _FileTransfer;
        private Task _TCPlistener;


        private CancellationTokenSource _cts;

        public TrayIconWindow()
        {
            InitializeComponent();
        }
        
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //Crea TrayIcon
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), //Directory where executable is (NEVER NULL)
                        "Media/switch.ico")
                )
            };
            //TODO Creare menu click tasto destro
            _trayIcon.DoubleClick += ExitApplication;
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();

            _comunication = new LanComunication();
            _comunication.StartLanAdvertise();
            _comunication.StartLanListen();

            ShowUsersWindow userWindow = new ShowUsersWindow();
            _comunication.UserFound += userWindow.AddUser;
            _comunication.UsersExpired += userWindow.RemoveUsers;
            userWindow.Closing += (o, a) => _comunication.UserFound -= userWindow.AddUser;
            userWindow.Closing += (o, a) => _comunication.UsersExpired -= userWindow.RemoveUsers;
            userWindow.Show();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cts.Cancel();
            _comunication.StopAll();
            Application.Current.Shutdown();
        }
        private void ExitApplication(object sender, EventArgs args)
        {
            _trayIcon.Visible = false;
            this.Close();
        }

    }
}
