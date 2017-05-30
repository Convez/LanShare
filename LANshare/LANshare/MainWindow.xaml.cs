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
using LANshare.Connection;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon trayIcon = null;
        private LAN_Comunication communication = new LAN_Comunication();
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
            trayIcon = new System.Windows.Forms.NotifyIcon();
            trayIcon.Icon = new System.Drawing.Icon("Media/switch.ico");
            trayIcon.Click += new EventHandler(IconClicked);
            trayIcon.DoubleClick += new EventHandler(IconDoubleClicked);
        }

        private void IconDoubleClicked(object sender, EventArgs args)
        {
            Application.Current.Shutdown();
        }
        private void IconClicked(object sender, EventArgs eventArgs)
        {
            Visibility = Visibility.Visible;
        }
    }
}
