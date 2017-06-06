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
            Console.WriteLine("Entered");
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                List.Items.Add(args[i]);
            }
        }

        public override void EndInit()
        {
            base.EndInit();
            _trayIcon.Visible = true;
            Visibility = Visibility.Visible;
        }

        protected override void OnInitialized(EventArgs e)
        {
            Console.WriteLine("Init");
            base.OnInitialized(e);
            Model.Configuration.LoadConfiguration();
            _trayIcon = new System.Windows.Forms.NotifyIcon {
                Icon = new System.Drawing.Icon(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), //Directory where executable is (NEVER NULL)
                        "Media/switch.ico")
                    )
            };
           
            _trayIcon.DoubleClick += IconDoubleClicked;
            _cts = new CancellationTokenSource();
            _comunication = new LanComunication();
            //_advertiser = Task.Run( async()=> { await _comunication.LAN_Advertise(_cts.Token); });
            //_listener = Task.Run(async () => { await Task.Run(()=>_comunication.LAN_Listen(_cts.Token)); });

            //var tcproba = new TCP_FileTransfer();
            //_advertiser = Task.Run(async () => await tcproba.TransferRequestListener(_cts.Token));
            //_listener = Task.Run(async () => await tcproba.TransferRequestSender(Model.Configuration.CurrentUser, new List<string>()));

        }

        private void IconDoubleClicked(object sender, EventArgs args)
        {
            _trayIcon.Visible = false;
            _cts.Cancel();
            //_advertiser.Wait();
            //_listener.Wait();
            Application.Current.Shutdown();
        }
        
    }
}
