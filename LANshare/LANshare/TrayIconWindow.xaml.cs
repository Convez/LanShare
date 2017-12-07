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
using LANshare.Model;
using LANshare.Properties;

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
        private System.Windows.Forms.MenuItem notSending;
        private System.Windows.Forms.MenuItem privacy;
        private System.Windows.Forms.MenuItem settings;
        private System.Windows.Forms.MenuItem exit;
        private int transfers = 0; //number of active transations
        private ContextMenu menu;




        private LanComunication _comunication;

        private TCP_Comunication _tcpComunication;


        private CancellationTokenSource _cts;

        public TrayIconWindow()
        {
            SetupNetwork();
            //SetupTrayIcon();
            InitializeComponent();
            menu = (ContextMenu)this.FindResource("NotifierContextMenu");
            //menu.DataContext = Model.Configuration.CurrentUser;
            menu.DataContext = new
            {
                Configuration.CurrentUser.PrivacyMode,
                transfers,
                
            };
            //this.DataContext = Model.Configuration.CurrentUser;
            //MenuItem pMenuIt = (MenuItem)this.FindResource("privacymi");
            //pMenuIt.DataContext = Configuration.CurrentUser;
            //pMenuIt.
        }

        public TrayIconWindow(StartupEventArgs e)
        {
            SetupNetwork();
            SetupTrayIcon();
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
        private void SetupTrayIcon()
        {
            //show_window = new System.Windows.Forms.MenuItem("Open LANgur Share", new EventHandler(delegate (Object sender, System.EventArgs args)
            //{
            //    try
            //    {
            //        ShowUsersWindow userWindow = OnButtonClick<ShowUsersWindow>();
            //        _comunication.UserFound += userWindow.AddUser;
            //        _comunication.UsersExpired += userWindow.RemoveUsers;
            //        userWindow.Closing += (o, a) => _comunication.UserFound -= userWindow.AddUser;
            //        userWindow.Closing += (o, a) => _comunication.UsersExpired -= userWindow.RemoveUsers;
            //    }
            //    catch(ArgumentNullException e)
            //    {
            //        Console.WriteLine("null window." + e.Message);
            //        //manage someway.. shutdown app?
            //    }
            // }));

            //privacy = new System.Windows.Forms.MenuItem("Private Mode: " + privateMode, new EventHandler(delegate (Object sender, System.EventArgs a)
            //{
            //    System.Windows.Forms.MenuItem m = sender as System.Windows.Forms.MenuItem;
            //    if (privateMode == "off")
            //    {
            //        privateMode = "on";
            //        is_private_mode = true;
            //    }
            //    else
            //    {
            //        privateMode = "off";
            //        is_private_mode = false;
            //    }
            //    m.Text = "Private Mode: " + privateMode;

            //}));

            //exit = new System.Windows.Forms.MenuItem("Exit", ExitApplication);

            sending = new System.Windows.Forms.MenuItem("View file transfer progress", new EventHandler(delegate (Object sender, System.EventArgs args)
            {
                TransfersWindow tw = OnButtonClick<TransfersWindow>();
                tw.Closing += (o, a) => RefreshSendingItem();
                //must subscribe to some _communication events and unsubscribe on closing

            }));

            notSending = new System.Windows.Forms.MenuItem("No active file transfers");

            //settings = new System.Windows.Forms.MenuItem("Settings", new EventHandler(delegate (Object sender, System.EventArgs args)
            //{
            //    OnButtonClick<SettingsWindow>();
                
            //}));

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
                        "Media/Images/ApplicationImages/switch.ico")
                )
            };
            _trayIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(notifier_MouseDown);
            //icon_menu = new System.Windows.Forms.ContextMenu();

            //icon_menu.MenuItems.Add(0, show_window);

            //icon_menu.MenuItems.Add(1, privacy);

            //icon_menu.MenuItems.Add(2, notSending);

            //icon_menu.MenuItems.Add(3, settings);

            //icon_menu.MenuItems.Add(4, exit);


            NotifyTransferOpened(); //debug only

            //_trayIcon.ContextMenu = icon_menu;

           
            //_trayIcon.DoubleClick += ExitApplication;
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();
            
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cts.Cancel();
            _comunication.StopAll();
            Application.Current.Shutdown();
        }
        private void ExitApplication(object sender, RoutedEventArgs args)
        {
            _trayIcon.Visible = false;
            this.Close();
        }

        private void StartSendingProcedure(object sender, List<string> args)
        {
            string s = "";
            args.ToList().ForEach(x => s += x + "\n");
            MessageBox.Show(s);
        }

        //called after transactions window is closed to reinsert relative menuitem in context menu
        private void RefreshSendingItem()
        {

            //int i = icon_menu.MenuItems.Count;
            //if (transfers == 1)
            //{
            //    bool ignoreAction = false;
            //    try { icon_menu.MenuItems.Remove(notSending); }

            //    catch (Exception e)
            //    {
            //        Console.WriteLine("error in removing element.\n" + e.Message);
            //        int w = Application.Current.Windows.OfType<TransfersWindow>().Count();  //check if there is an istance of the window open
            //        if (icon_menu.MenuItems.Contains(sending) || w == 1) ignoreAction = true; //in caso non si apresente l'elemento notSending è altamente probabile sia invece presente l'elemento sending che verrà rimosso per non crearne molteplici 
            //    }

            //    finally
            //    {
            //        if (!ignoreAction)
            //        {
            //            if (icon_menu.MenuItems.Contains(settings)) icon_menu.MenuItems.Add(i - 2, sending);
            //            else icon_menu.MenuItems.Add(i - 1, sending);
            //        }
            //    }

            //}
            //else if (transfers == 0)
            //{
            //    bool ignoreAction = false;
            //    try { icon_menu.MenuItems.Remove(sending); }

            //    catch (Exception e)
            //    {
            //        Console.WriteLine("error in removing element.\n" + e.Message);
            //        int w = Application.Current.Windows.OfType<TransfersWindow>().Count();
            //        if (icon_menu.MenuItems.Contains(sending) || w == 1) ignoreAction = true;
            //    }

            //    finally
            //    {
            //        if (!ignoreAction)
            //        {
            //            if (icon_menu.MenuItems.Contains(settings)) icon_menu.MenuItems.Add(i - 2, notSending);
            //            else icon_menu.MenuItems.Add(i - 1, notSending);
            //        }
            //    }

            //}

        }

   
        // to be called when a new transfer (one way stream of files that are either sent to the same user or received from the same user) is set up.. 
        //so we might at most have 2 different transfer connected to another user, one of the files being sent and one of the files being received. 
        //group transactions are not considered for the sake of the transactions counter as they are counted separately for each of the participants in the group.

        public void NotifyTransferOpened()
        {
            transfers++;
            //if (transfers == 1)
            //{
            //    RefreshSendingItem();
            //}
        }

        //to be called when a transfer has finished (a transfer is considered finished when all the files that were being sent to (or received from) a certain user have successfully completed
        public void NotifyTransferClosed()
        {
            try
            {
                transfers--;
                if (transfers < 0)
                {
                    //an error has occured, recheck number of transactions
                    throw new ApplicationException("error in transfer count");
                }
                //else if (transfers == 0)
                //{
                //    RefreshSendingItem();
                //}
            }
            catch (ApplicationException e)
            {
                Console.WriteLine("ERROR: transfers count went negative" + e.Message);
                //get actual transfers number here
                //if (transfers == 0) RefreshSendingItem();

            }


        }
        private T OnButtonClick<T>() where T: Window, new()
        {
            int w = Application.Current.Windows.OfType<T>().Count();
            if (w == 1)
            {
                Application.Current.Windows.OfType<T>().First().Activate();
                return Application.Current.Windows.OfType<T>().First();
            }
            else if (w > 1)
            {
                while (w > 1)
                {
                    Application.Current.Windows.OfType<T>().First().Close();
                    w = Application.Current.Windows.OfType<T>().Count();
                }
                Application.Current.Windows.OfType<T>().First().Activate();
                return Application.Current.Windows.OfType<T>().First();
            }
            else if (w == 0)
            {
                T window = new T();
                window.Show();
                return window;
            }
            return null;
        }

        private void ShowPeople(object sender, RoutedEventArgs e)
        {
            ShowUsersWindow userWindow = OnButtonClick<ShowUsersWindow>();
            _comunication.UserFound += userWindow.AddUser;
            _comunication.UsersExpired += userWindow.RemoveUsers;
            userWindow.Closing += (o, a) => _comunication.UserFound -= userWindow.AddUser;
            userWindow.Closing += (o, a) => _comunication.UsersExpired -= userWindow.RemoveUsers;
        }

        private void SetPrivacy(object sender, RoutedEventArgs e)
        {
            Configuration.CurrentUser.SetPrivacyMode();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            OnButtonClick<SettingsWindow>();
        }
        private void OpenTransfers(object sender, RoutedEventArgs e)
        {
            OnButtonClick<TransfersWindow>();
        }

        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open");
        }

        private void Menu_Close(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Close");
        }
        void notifier_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (!menu.IsOpen) menu.IsOpen = true;
                else menu.IsOpen = false;
            }
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowPeople(sender, null);
                OpenTransfers(sender, null);
            }
        }



    }
}
