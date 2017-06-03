using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using LANshare.Connection;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private LanComunication _comunication;
        private Task _advertiser;
        private Task _listener;
        private CancellationTokenSource _cts;
        public MainWindow()
        {
            InitializeComponent();

            //TODO Use netstat -n -o to find pid of process binded to port

        }

        public override void EndInit()
        {
            base.EndInit();
            _trayIcon.Visible = true;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Model.Configuration.LoadConfiguration();
            _trayIcon = new System.Windows.Forms.NotifyIcon {Icon = new System.Drawing.Icon("Media/switch.ico")};
            _trayIcon.DoubleClick += new EventHandler(IconDoubleClicked);
            _cts = new CancellationTokenSource();
            _comunication = new LanComunication();
            //_advertiser = Task.Run( async()=> { await _comunication.LAN_Advertise(_cts.Token); });
            //_listener = Task.Run(async () => { await Task.Run(()=>_comunication.LAN_Listen(_cts.Token)); });

            var tcproba = new TCP_FileTransfer();
            _advertiser = Task.Run(async () => await tcproba.TransferRequestListener(_cts.Token));
            _listener = Task.Run(async () => await tcproba.TransferRequestSender(Model.Configuration.CurrentUser, new List<string>()));

        }

        private void IconDoubleClicked(object sender, EventArgs args)
        {
            _trayIcon.Visible = false;
            _cts.Cancel();
            _advertiser.Wait();
            _listener.Wait();
            Application.Current.Shutdown();
        }
        
    }
}
