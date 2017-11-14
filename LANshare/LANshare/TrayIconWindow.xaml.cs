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
        private String privateMode = "off"; //there should be some function to link is_privacy_mode to priv and also to set  certain value as default at startup
        private System.Windows.Forms.MenuItem show_window;
        private System.Windows.Forms.MenuItem sending;
        private System.Windows.Forms.MenuItem privacy;
        private System.Windows.Forms.MenuItem exit;
        private int transfers = 0; //number of active transations


        
        private LanComunication _comunication;

        private TCP_Comunication _tcpComunication;


        private CancellationTokenSource _cts;

        public TrayIconWindow()
        {
            SetupNetwork();
            show_window = new System.Windows.Forms.MenuItem("Open LANgur Share", new EventHandler(delegate (Object sender, System.EventArgs args)
            {
                ShowUsersWindow userWindow = new ShowUsersWindow();
                _comunication.UserFound += userWindow.AddUser;
                _comunication.UsersExpired += userWindow.RemoveUsers;
                userWindow.Closing += (o, a) => _comunication.UserFound -= userWindow.AddUser;
                userWindow.Closing += (o, a) => _comunication.UsersExpired -= userWindow.RemoveUsers;
                userWindow.Show();
                icon_menu.MenuItems.RemoveAt(0); //menuitem is removed to avoid opening multiple instances of the users window       

            }));

            privacy = new System.Windows.Forms.MenuItem("Private Mode: " + privateMode, new EventHandler(delegate (Object sender, System.EventArgs a)
            {
                System.Windows.Forms.MenuItem m = sender as System.Windows.Forms.MenuItem;
                if (privateMode == "off")
                {
                    privateMode = "on";
                    is_private_mode = true;
                }
                else
                {
                    privateMode = "off";
                    is_private_mode = false;
                }
                m.Text = "Private Mode: " + privateMode;

            }));

            exit = new System.Windows.Forms.MenuItem("Exit", ExitApplication);

            sending = new System.Windows.Forms.MenuItem("View file transfer progress", new EventHandler(delegate (Object sender, System.EventArgs a)
            {

                var transfersWindow = new TransfersWindow(this);
                transfersWindow.Show();
                icon_menu.MenuItems.RemoveAt(3);

            }));
            InitializeComponent();
        }

        public TrayIconWindow(StartupEventArgs e)
        {
            SetupNetwork();
            show_window = new System.Windows.Forms.MenuItem("Open LANgur Share", new EventHandler(delegate (Object sender, System.EventArgs a)
            {

                var userWindow = new ShowUsersWindow();
                userWindow.Show();
                icon_menu.MenuItems.RemoveAt(0); //menuitem is removed to avoid opening multiple instances of the users window       

            }));

            privacy = new System.Windows.Forms.MenuItem("Private Mode: " + privateMode, new EventHandler(delegate (Object sender, System.EventArgs a)
            {
                System.Windows.Forms.MenuItem m = sender as System.Windows.Forms.MenuItem;
                if (privateMode == "off")
                {
                    privateMode = "on";
                    is_private_mode = true;
                }
                else
                {
                    privateMode = "off";
                    is_private_mode = false;
                }
                m.Text = "Private Mode: " + privateMode;

            }));

            exit = new System.Windows.Forms.MenuItem("Exit", ExitApplication);

            sending = new System.Windows.Forms.MenuItem("View file transfer progress", new EventHandler(delegate (Object sender, System.EventArgs a)
            {

                var transfersWindow = new TransfersWindow();
                //must subscribe to events
                transfersWindow.Show();
                icon_menu.MenuItems.RemoveAt(3);

            }));
            InitializeComponent();
            StartSendingProcedure(this, e.Args.ToList());

        }

        private void SetupNetwork()
        {
            _comunication = new LanComunication();
            _comunication.StartLanAdvertise();
            _comunication.StartLanListen();
            _tcpComunication = new TCP_Comunication();
            _tcpComunication.StartTcpServers();
            _tcpComunication.fileSendRequested += StartSendingProcedure;
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
            icon_menu = new System.Windows.Forms.ContextMenu();

            icon_menu.MenuItems.Add(0, show_window);

            icon_menu.MenuItems.Add(1, privacy);

            icon_menu.MenuItems.Add(2, exit);
            NotifyTransferOpened();

            _trayIcon.ContextMenu = icon_menu;


            //TODO Creare menu click tasto destro
            //_trayIcon.DoubleClick += ExitApplication;
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();
            //TODO Creare menu click tasto destro
            _trayIcon.DoubleClick += ExitApplication;
            _trayIcon.Visible = true;


            //Showcase utilizzo enviroment di rete
            //ShowUsersWindow userWindow = new ShowUsersWindow();
            //_comunication.UserFound += userWindow.AddUser;
            //_comunication.UsersExpired += userWindow.RemoveUsers;
            //userWindow.Closing += (o, a) => _comunication.UserFound -= userWindow.AddUser;
            //userWindow.Closing += (o, a) => _comunication.UsersExpired -= userWindow.RemoveUsers;
            //userWindow.Show();
            
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

        private void StartSendingProcedure(object sender, List<string> args)
        {
            string s = "";
            args.ToList().ForEach(x => s += x+"\n");
            MessageBox.Show(s);
        }

        public void RestoreShowWindowItem()
        {
            icon_menu.MenuItems.Add(0, show_window);
        } //called after the users window is closed to reinsert the relative menu item in context menu

        //called after transactions window is closed to reinsert relative menuitem in context menu
        public void RestoreSendingItem()
        {
            if (transfers > 0) icon_menu.MenuItems.Add(3, sending);
        }

        // to be called when a new transfer (one way stream of files that are either sent to the same user or received from the same user) is set up.. 
        //so we might at most have 2 different transfer connected to another user, one of the files being sent and one of the files being received. 
        //group transactions are not considered for the sake of the transactions counter as they are counted separately for each of the participants in the group.

        public void NotifyTransferOpened()
        {
            if (transfers == 0)
            {
                icon_menu.MenuItems.Add(3, sending);
            }
            transfers++;

        }

        //to be called when a transfer has finished (a transfer is considered finished when all the files that were being sent to (or received from) a certain user have successfully completed
        public void NotifyTransferClosed()
        {
            transfers--;
            if (transfers < 0)
            {
                //an error has occured, recheck number of transactions
                Console.WriteLine("error in transaction count");
            }
            else if (transfers == 0)
            {
                icon_menu.MenuItems.RemoveAt(3);
            }

        }

    }
}
