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

namespace LANshare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon trayIcon = null;
        public MainWindow()
        {
            InitializeComponent();
            
            
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
            
            
        }
        
        private void IconClicked(object sender, EventArgs eventArgs)
        {
            Visibility = Visibility.Visible;
        }
    }
}
