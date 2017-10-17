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
        private bool is_private_mode = false; //should add the option to set true as default at startup
        private System.Windows.Forms.MenuItem show_window;

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

            show_window = new System.Windows.Forms.MenuItem("Open LANgur Share", new EventHandler(delegate (Object sender, System.EventArgs a)
            {

                var userWindow = new ShowUsersWindow(this);
                userWindow.Show();
                icon_menu.MenuItems.RemoveAt(0); //menuitem is removed to avoid opening multiple instances of the users window       

            }));

            icon_menu.MenuItems.Add(0, show_window);

            String priv = "off"; //there should be some function to link is_privacy_mode to priv and also to set  certain value as default at startup

            System.Windows.Forms.MenuItem privacy = new System.Windows.Forms.MenuItem("Private Mode: "+ priv , new EventHandler(delegate (Object sender, System.EventArgs a)
            {
                System.Windows.Forms.MenuItem m = sender as System.Windows.Forms.MenuItem;
                if( priv== "off")
                {
                    priv = "on";
                    is_private_mode = true;
                }
                else
                {
                    priv = "off";
                    is_private_mode = false;
                }
                m.Text = "Private Mode: " + priv;
                
            }));

            icon_menu.MenuItems.Add(1, privacy);

            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("Exit",ExitApplication);

            icon_menu.MenuItems.Add(2, exit);

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

        public void RestoreItem()
        {
            icon_menu.MenuItems.Add(0, show_window);
        }
    }
}
