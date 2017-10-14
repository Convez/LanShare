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
        private System.Windows.Forms.ContextMenu icon_menu;

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
            _trayIcon.Visible = true;
            //Crea Context menu del trayicon
            icon_menu = new System.Windows.Forms.ContextMenu();
            //MenuItem show_interface = new MenuItem() { Header = "Open LANgur Share" };
            //show_interface.Click += new RoutedEventHandler(delegate (Object o, RoutedEventArgs a) //+= notation subscribes to event
            //{
            //    //code to show users window
            //    var userWindow = new ShowUsersWindow();
            //    userWindow.Show();
            //});
            icon_menu.MenuItems.Add(0, new System.Windows.Forms.MenuItem("Open LANgur Share", new EventHandler(delegate (Object sender, System.EventArgs a)
                 {
                     var userWindow = new ShowUsersWindow();
                     userWindow.Show();
                     icon_menu.MenuItems.RemoveAt(0);
                 })));

            icon_menu.MenuItems.Add(1, new System.Windows.Forms.MenuItem("Exit", new EventHandler(delegate (Object sender, System.EventArgs a)
            {
                Application.Current.Shutdown();
                //icon bug after closing the application must be fixed
            })));

            _trayIcon.ContextMenu = icon_menu;
            

            //TODO Creare menu click tasto destro
            //_trayIcon.DoubleClick += ExitApplication;
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();

            _comunication = new LanComunication();

            //Crea thread per mandare pacchetti di advertisement
            _UDPadvertiser = Task.Run(async () => { await _comunication.LAN_Advertise(_cts.Token); });
            
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cts.Cancel();
            _UDPadvertiser.Wait();
            Application.Current.Shutdown();
        }
        private void ExitApplication(object sender, EventArgs args)
        {
            _trayIcon.Visible = false;
            this.Close();
        }

    }
}
