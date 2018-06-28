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
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

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
        private int _menuX;
        private int _menuY;
        private int _privacyMenuItemPosition = 1; //this must be update if position in the privacy menu item is changed in xaml
        private int _transfersMenuItemPosition = 2;
        private int _notifications;
        //private ObservableCollection<Notification> _notificationsList;
        private Icon _icon;
        public int Transfers { get => _transfers;
            set {
                
                _transfers = value;
                this.Dispatcher.Invoke(() =>
                {
                    MenuItem m = (MenuItem)_menu.Items[_transfersMenuItemPosition];
                    m.Header = _transfers;
                });
                
            } }

        private List<IFileTransferHelper> ongoingTransfers = new List<IFileTransferHelper>();

        private LanComunication _comunication;

        private TCP_Comunication _tcpComunication;

        private CancellationTokenSource _cts;

        public event EventHandler<IFileTransferHelper> addedToTransfers;
        public event EventHandler<IFileTransferHelper> removedFromTransfers;

        
        public TrayIconWindow()
        {
            SetupApplication();
        }

        public TrayIconWindow(StartupEventArgs e)
        {
            SetupApplication();

            StartSendingProcedure(this, e.Args.ToList());
        }

        private void SetupApplication() {
            SetupNetwork();
            InitializeComponent();
            _menu = (ContextMenu)this.FindResource("NotifierContextMenu");
            Configuration.CurrentUser.PropertyChanged += PrivacyBinding;
            _menu.DataContext = new
            {
                Configuration.CurrentUser.PrivacyMode,
                Transfers,

            };

            _tcpComunication.UploadAccepted += (o, a) => NewTransfer(a);
            _tcpComunication.TransferRequested += (o, a) =>
            {
                
                Notification n = new Notification("", Notification.NotificationType.transferRequest, a);
                NewNotification(n);
            };

            _tcpComunication.requestSeen += (o, a) => OnNotificationClosed(o, a);

            _menu.Loaded += new RoutedEventHandler(Menu_Loaded);
            _menu.Unloaded += new RoutedEventHandler(Menu_Unloaded);
            _proc = HookCallback;
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
                        "Media/Images/ApplicationImages/logosimple.ico"));
            //Crea TrayIcon
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = _icon
            };

            _trayIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(notifier_MouseDown);
            
            _trayIcon.Visible = true;
            _cts = new CancellationTokenSource();
            _notifications =0;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _cts.Cancel();
            _comunication.StopAll();
            _tcpComunication.StopAll();
            _tcpComunication.StopLoopback();
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
                ShowUsersWindowDLL suw = new ShowUsersWindowDLL();
                _comunication.UserFound += suw.AddUser;
                _comunication.UsersExpired += suw.RemoveUsers;
                suw.Closing += (o, a) => _comunication.UserFound -= suw.AddUser;
                suw.Closing += (o, a) => _comunication.UsersExpired -= suw.RemoveUsers;
                suw.UsersSelected += (send, argx) => 
                {
                    List<User> l = (List<User>)argx.Args[0];
                    StartUpload(args, l, argx.Args[1] as string);
                    suw.Close();
                };
                suw.setList(_comunication.GetUsers());
                suw.Show();
            });
        }

        private void StartUpload(List<string>what, List<User> to, string mode) //mode is "file" if file must be sent, "folder" if folder
        {
            //TODO TEST
            if (what.Count == 0)
            {

                if (mode == "file")
                {
                    System.Windows.Forms.OpenFileDialog openFileDialog;
                    openFileDialog = new System.Windows.Forms.OpenFileDialog();
                    openFileDialog.Title = "Select files";
                    openFileDialog.Multiselect = true;
                    System.Windows.Forms.DialogResult dr = openFileDialog.ShowDialog();

                    if (dr == System.Windows.Forms.DialogResult.OK)
                    {
                        var names = openFileDialog.FileNames;
                        string basePath = Path.GetDirectoryName(names.First());
                        what.Add(basePath);
                        foreach (String file in names)
                        {
                            what.Add(Path.GetFileName(file));
                        }
                    }
                    else return;
                }
                else if (mode == "folder")
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
                    folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderBrowserDialog.Description = "Select a folder";

                    System.Windows.Forms.DialogResult dr = folderBrowserDialog.ShowDialog();
                    if (dr == System.Windows.Forms.DialogResult.OK)

                    {
                        string path = folderBrowserDialog.SelectedPath;
                        DirectoryInfo info = new DirectoryInfo(path);
                        //string basepath = Directory.GetParent(path).FullName;
                        what.Add(info.Parent.FullName);
                        what.Add(info.Name);
                    }

                }


            }

            foreach (User u in to)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    FileUploadHelper uploader = new FileUploadHelper();
                    uploader.Counterpart = u;

                    if (File.Exists(Path.Combine("tmp", u.SessionId + ".jpg")))
                    {
                        u.ProfilePicture = new BitmapImage(new Uri(Path.Combine("tmp", u.SessionId + ".jpg"), UriKind.Relative));
                    }
                    else
                    {
                        u.SetupImage();
                    }
                    uploader.Status = TransferCompletitionStatus.Requested;
                    NewTransfer(uploader);
                    bool accepted=uploader.InitFileSend(u, what, cts.Token);                    
                });
            }

            
        }

        private void NewTransfer(IFileTransferHelper h)
        {
            Dispatcher.Invoke(() =>
            {
                OpenTransfers(this, null);

            });
            h.cancelRequested += (o, a) => RemoveTransaction(h);
            h.TransferCompleted += (o, a) => RemoveTransaction(h);
            ongoingTransfers.Add(h);
            OnAddedToTransfers(h);
            

        }

        private void RemoveTransaction(IFileTransferHelper e)
        {
            ongoingTransfers.Remove(e);
            OnRemovedFromTransfers(e);
            
        }


        public static T OpenWindow<T, Y>(List<Y> y)
            where T : Window, ListWindow<Y>, new()
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
                window.setList(y);
                window.Show();
                return window;
            }
            return null;
        }

        private void ShowPeople(object sender, EventArgs e)
        {
            ShowUsersWindow userWindow = OpenWindow<ShowUsersWindow,User>(_comunication.GetUsers());
            if (!userWindow.isSubscribed)
            {
                userWindow.isSubscribed = true;
                _comunication.UserFound += userWindow.AddUser;
                _comunication.UsersExpired += userWindow.RemoveUsers;
                userWindow.transfersButtonClick += OpenTransfers;
                userWindow.settingsButtonClick += OpenSettings;
                userWindow.UsersSelected += (send, args) =>
                {
                    List<User> l = (List<User>)args.Args[0];
                    StartUpload(new List<String>(), l.Distinct().ToList(), args.Args[1] as string);
                };
                userWindow.Closing += (o, a) =>
                {
                    userWindow.isSubscribed = false;
                    _comunication.UserFound -= userWindow.AddUser;
                    _comunication.UsersExpired -= userWindow.RemoveUsers;
                    userWindow.transfersButtonClick -= OpenTransfers;
                    userWindow.settingsButtonClick -= OpenSettings;
                };
            }
        }


        private void OpenSettings(object sender, EventArgs e)
        {
            SettingsWindow settw = OpenWindow<SettingsWindow, User>(null);
            if (!settw.isSubscribed)
            {
                settw.isSubscribed = true;
                settw.peopleButtonClick += ShowPeople;
                settw.transfersButtonClick += OpenTransfers;
                settw.privacyChanged += SetPrivacy;
                settw.Closing += (o, a) =>
                {
                    settw.isSubscribed = false;
                    settw.transfersButtonClick -= OpenTransfers;
                    settw.settingsButtonClick -= ShowPeople;
                    settw.privacyChanged -= SetPrivacy;
                };
            }
        }
        private void OpenTransfers(object sender, EventArgs e)
        {
            TransfersWindow tf= OpenWindow<TransfersWindow, IFileTransferHelper>(ongoingTransfers);
            if (!tf.isSubscribed)
            {
                tf.isSubscribed = true;
                tf.peopleButtonClick += (o, a) => ShowPeople(this, null);
                tf.settingsButtonClick += (o, a) => OpenSettings(this, null);
                addedToTransfers += tf.AddTransfer;
                tf.Closing += (o, a) =>
                {
                    tf.isSubscribed = false;
                    tf.transfersButtonClick -= ShowPeople;
                    tf.settingsButtonClick -= OpenSettings;
                    addedToTransfers -= tf.AddTransfer;
                };
            }
        }

        private void SetPrivacy(object sender, EventArgs e)
        {
            Configuration.CurrentUser.SetPrivacyMode();
            if (Configuration.CurrentUser.PrivacyMode == "Public")
                _comunication.StartLanAdvertise();
            else
                _comunication.GoPrivate();

        }



        private void notifier_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.Clicks==1)
            {
                if (!_menu.IsOpen) {
                    
                    _menuX = System.Windows.Forms.Cursor.Position.X;
                    _menuY = System.Windows.Forms.Cursor.Position.Y;
                   
                    _menu.IsOpen = true;
                } 
                else _menu.IsOpen = false;
            }
            if(e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowPeople(sender, null);
            }
            else if(e.Button== System.Windows.Forms.MouseButtons.Middle )
            {

                double w = _menu.ActualWidth; 
                double h = _menu.ActualHeight;

                double centerX = _menu.RenderTransformOrigin.X;
               
                
                if(e.X<_menuX -w || e.X> (_menuX + w))
                {
                    if(e.Y<_menuY || (e.Y> _menuY+h)) _menu.IsOpen = false; //checks wether the click is outside menu
                }
                    
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
            //_notificationsList.Add(notification);
            _notifications++;
            System.Media.SystemSounds.Exclamation.Play();

            UnseenNotificationsIconOverlay(true);
        }

        public void OnNotificationClosed(object sender, EventArgs a)
        {
            if (_notifications >0) _notifications--;

            if(_notifications==0)    UnseenNotificationsIconOverlay(false);
                
            
        }


        // sets an overlay on the trayicon with the number of notifications in the notification queue
        private void UnseenNotificationsIconOverlay(bool notificationsUnseen)
        {

            string n;
            

            
            Graphics canvas;
            Bitmap iconBitmap = new Bitmap(32, 32);
            canvas = Graphics.FromImage(iconBitmap);

            canvas.DrawIcon(_icon, 0, 0);

            if (notificationsUnseen)
            {
                n = "!";

                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                Font f = new Font("Goudy Stout", 16.00f);
                Pen p = new Pen(new SolidBrush(System.Drawing.Color.Green));

                //canvas.DrawEllipse(new Pen(new SolidBrush(System.Drawing.Color.Green)), new RectangleF(0, 0, 20, 20));
                canvas.FillEllipse(new SolidBrush(System.Drawing.Color.White), new RectangleF(9, 9, 22, 22));

                canvas.DrawString(
                    n,
                    f,
                    new SolidBrush(System.Drawing.Color.Blue),
                    new RectangleF(9, 7, 21, 21),
                    format
                );

            }

            _trayIcon.Icon = System.Drawing.Icon.FromHandle(iconBitmap.GetHicon());

        }


        public void OnAddedToTransfers(IFileTransferHelper e) {
            addedToTransfers?.Invoke(this, e);
            Transfers = ongoingTransfers.Count();
            
        }

        public void OnRemovedFromTransfers(IFileTransferHelper e)
        {
            removedFromTransfers?.Invoke(this, e);
            Transfers = ongoingTransfers.Count();
           
        }



        //the following methods are used for managing clicks outside of the application elements


        void Menu_Loaded(object sender, RoutedEventArgs e)
        {
            _hookID = SetHook(_proc);
        }

        void Menu_Unloaded(object sender, RoutedEventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
        }

        private LowLevelMouseProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                  GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private  IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                
                notifier_MouseDown(this, new System.Windows.Forms.MouseEventArgs( System.Windows.Forms.MouseButtons.Middle, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
          IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


    }
}
