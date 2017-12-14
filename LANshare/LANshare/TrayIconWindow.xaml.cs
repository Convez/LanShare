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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Interop;
using System.Drawing;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for TrayIconWindow.xaml
    /// </summary>
    public partial class TrayIconWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private int _transfers = 0; //number of active transations
        private ContextMenu _menu;
        private int _privacyMenuItemPosition = 1; //this must be update if position in the privacy menu item is changed in xaml
        private LinkedList<Notification> _notificationsQueue;

        private LanComunication _comunication;

        private TCP_Comunication _tcpComunication;

        private CancellationTokenSource _cts;

        public TrayIconWindow()
        {
            SetupNetwork();
            InitializeComponent();
            _menu = (ContextMenu)this.FindResource("NotifierContextMenu");
            Configuration.CurrentUser.PropertyChanged += PrivacyBinding;
            _menu.DataContext = new
            {
                Configuration.CurrentUser.PrivacyMode,
                _transfers,

            };
        }

        public TrayIconWindow(StartupEventArgs e)
        {
            SetupNetwork();
            InitializeComponent();
            _menu = (ContextMenu)this.FindResource("NotifierContextMenu");
            Configuration.CurrentUser.PropertyChanged += PrivacyBinding;
            _menu.DataContext = new
            {
                Configuration.CurrentUser.PrivacyMode,
                _transfers,

            };
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
                        "Media/Images/ApplicationImages/switch.ico")
                )
            };

            _trayIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(notifier_MouseDown);
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();
            _notificationsQueue = new LinkedList<Notification>();
            UnseenNotificationsPopup();
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
 
   
        // to be called when a new transfer (one way stream of files that are either sent to the same user or received from the same user) is set up.. 
        //so we might at most have 2 different transfer connected to another user, one of the files being sent and one of the files being received. 
        //group transactions are not considered for the sake of the transactions counter as they are counted separately for each of the participants in the group.

        public void NotifyTransferOpened()
        {
            _transfers++;
        }

        //to be called when a transfer has finished (a transfer is considered finished when all the files that were being sent to (or received from) a certain user have successfully completed
        public void NotifyTransferClosed()
        {
            try
            {
                _transfers--;
                if (_transfers < 0)
                {
                    //an error has occured, recheck number of transactions
                    throw new ApplicationException("error in transfer count");
                }
                
            }
            catch (ApplicationException e)
            {
                Console.WriteLine("ERROR: transfers count went negative" + e.Message);
                //get actual transfers number here
               

            }


        }
        private T OpenWindow<T>() where T: Window, new()
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
            ShowUsersWindow userWindow = OpenWindow<ShowUsersWindow>();
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
            OpenWindow<SettingsWindow>();
        }
        private void OpenTransfers(object sender, RoutedEventArgs e)
        {
            OpenWindow<TransfersWindow>();
        }


        void notifier_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (!_menu.IsOpen) _menu.IsOpen = true;
                else _menu.IsOpen = false;
            }
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowPeople(sender, null);
                OpenTransfers(sender, null);
            }
        }

        private void PrivacyBinding(object sender , PropertyChangedEventArgs e)
        {
            //MenuItem m = (MenuItem)menu.FindName("PrivacyItem");
            MenuItem m = (MenuItem) _menu.Items[_privacyMenuItemPosition];
            String mode= Configuration.CurrentUser.PrivacyMode;
            m.Header = mode;
        }

        public void NewNotification(Notification notification)
        {
            if(notification.MsgType==Notification.NotificationType.transferRequest || notification.MsgType == Notification.NotificationType.transferAbort)
            {
                System.Media.SystemSounds.Exclamation.Play();
            }
            int w = Application.Current.Windows.OfType<NotificationWindow>().Count();
            if (w == 1)
            {
                _notificationsQueue.AddLast(notification);
                NotificationWindow n= Application.Current.Windows.OfType<NotificationWindow>().First();
                n.NewNotificationInQueue();

            }
            else if (w == 0)
            {
                NotificationWindow n = new NotificationWindow(notification, _notificationsQueue.Count());
                n.Closing += OnNotificationClosed;
                n.Show();
            }
        }

        public void OnNotificationClosed(object sender, CancelEventArgs a)
        {  
            _notificationsQueue.RemoveFirst();
            if(_notificationsQueue.Count>0)
            {
                NotificationWindow n = new NotificationWindow( _notificationsQueue.First() , _notificationsQueue.Count());
                n.Closing += OnNotificationClosed;
                n.Show();
                
            }
        }

        private void UnseenNotificationsPopup()
        {
            //RectangleF rectF = new RectangleF(0, 0, 40, 40);
            //Bitmap bitmap = new Bitmap(40, 40, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //Graphics g = Graphics.FromImage(bitmap);
            //g.FillRectangle(System.Drawing.Brushes.White, 0, 0, 40, 40);
            //g.DrawString("5", new Font("Arial", 25), System.Drawing.Brushes.Black, new PointF(0, 0));

            //IntPtr hBitmap = bitmap.GetHbitmap();

            //ImageSource wpfBitmap =
            //    Imaging.CreateBitmapSourceFromHBitmap(
            //        hBitmap, IntPtr.Zero, Int32Rect.Empty,
            //        BitmapSizeOptions.FromEmptyOptions());

            //TaskbarItemInfo.Overlay = wpfBitmap;
            //_trayIcon.
            Graphics canvas;
            Bitmap iconBitmap = new Bitmap(16, 16);
            canvas = Graphics.FromImage(iconBitmap);

            canvas.DrawIcon(YourProject.Resources.YourIcon, 0, 0);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;

            canvas.DrawString(
                "2",
                new Font( Gadugi , System.Drawing.FontStyle.Bold),
                new SolidBrush(Color.FromArgb(40, 40, 40)),
                new RectangleF(0, 3, 16, 13),
                format
            );

            _trayIcon.Icon = Icon.FromHandle(iconBitmap.GetHicon());

        }

    }
}
