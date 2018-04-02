using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using LANshare.Connection;
using LANshare.Model;
using LANshare.Properties;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Controls;


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
        private Icon _icon;

        private List<IFileTransferHelper> ongoingTransfers = new List<IFileTransferHelper>();

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
            _tcpComunication.FileSendRequested += StartSendingProcedure;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _icon = new Icon(System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, //Directory where executable is (NEVER NULL)
                        "Media/Images/ApplicationImages/switch.ico"));
            //Crea TrayIcon
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = _icon
            };

            _trayIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(notifier_MouseDown);
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();
            _notificationsQueue = new LinkedList<Notification>();
            UnseenNotificationsIconOverlay();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cts.Cancel();
            _comunication.StopAll();
            _tcpComunication.StopAll();
            Application.Current.Shutdown();
        }
        private void ExitApplication(object sender, RoutedEventArgs args)
        {
            _trayIcon.Visible = false;
            this.Close();
        }

        private void StartSendingProcedure(object sender, List<string> args)
        {
            Dispatcher.Invoke(() =>
            {
                ShowUsersWindow suw = new ShowUsersWindow(_comunication.GetUsers());
                _comunication.UserFound += suw.AddUser;
                _comunication.UsersExpired += suw.RemoveUsers;
                suw.Closing += (o, a) => _comunication.UserFound -= suw.AddUser;
                suw.Closing += (o, a) => _comunication.UsersExpired -= suw.RemoveUsers;
                suw.UsersSelected += (send, arg) =>
                {
                    StartUpload(args, arg);
                };
            });
        }

        private void StartUpload(List<string>what, List<User> to)
        {
            foreach(User u in to)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    FileUploadHelper uploader = new FileUploadHelper();
                    ongoingTransfers.Add(uploader);
                    uploader.InitFileSend(u, what, cts.Token);
                });
            }
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
            int w = Application.Current.Windows.OfType<NotificationWindow>().Count(); //se w=1 vuol dire che c'è una finestra già aperta che aspetta di essere clickata o chiusa ed eventualmente anche altre notifiche in coda
            if (w == 1)
            {
                _notificationsQueue.AddLast(notification);
                NotificationWindow n= Application.Current.Windows.OfType<NotificationWindow>().First();
                n.NewNotificationInQueue();
                UnseenNotificationsIconOverlay();

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
                NotificationWindow n = new NotificationWindow( _notificationsQueue.First() , _notificationsQueue.Count()); //here i will have to check if the next notification is a user online notification.. if so i have to check if that user is still online and if it makes sense to display the notification at all
                n.Closing += OnNotificationClosed;
                n.Show();
                UnseenNotificationsIconOverlay();
                
            }
        }


        // sets an overlay on the trayicon with the number of notifications in the notification queue
        private void UnseenNotificationsIconOverlay()
        {
            int n = 0;
            foreach (Notification nf in _notificationsQueue)
            {
                if (nf.MsgType != Notification.NotificationType.userOnline) n++; //user online notifications are not very important and should not be considered in the overlay
            }

            //devo controllare se n=0.. in caso positivo ripristino l'icona pulita senza overlay
 
            Graphics canvas;
            Bitmap iconBitmap = new Bitmap(32, 32);
            canvas = Graphics.FromImage(iconBitmap);

            canvas.DrawIcon(_icon, 0, 0);

            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            Font f = new Font("Verdana Pro Black", 18.00f);

            canvas.DrawString(
                n.ToString(),
                f,
                new SolidBrush(System.Drawing.Color.Red),
                new RectangleF(9, 5, 30, 30),
                format
            );

            _trayIcon.Icon = System.Drawing.Icon.FromHandle(iconBitmap.GetHicon());

        }

    }
}
