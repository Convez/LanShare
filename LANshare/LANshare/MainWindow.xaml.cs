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
using System.Threading.Tasks;
using LANshare.Connection;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon trayIcon = null;
        private LAN_Comunication comunication;
        private Task advertiser;
        private CancellationTokenSource cts;
        public MainWindow()
        {
            InitializeComponent();

            //TODO Use netstat -n -o to find pid of process binded to port

        }

        public override void EndInit()
        {
            base.EndInit();
            trayIcon.Visible = true;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Model.Configuration.LoadConfiguration();
            trayIcon = new System.Windows.Forms.NotifyIcon {Icon = new System.Drawing.Icon("Media/switch.ico")};
            trayIcon.DoubleClick += new EventHandler(IconDoubleClicked);
            cts = new CancellationTokenSource();
            comunication = new LAN_Comunication();
            advertiser = Task.Run(()=> { comunication.LAN_Advertise(cts.Token); });
                       
        }

        private void IconDoubleClicked(object sender, EventArgs args)
        {
            trayIcon.Visible = false;
            cts.Cancel();
            advertiser.Wait();
            Application.Current.Shutdown();
        }
        
    }
}
