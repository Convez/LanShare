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
        private Task _UDPadvertiser;

        private TCP_FileTransfer _FileTransfer;
        private Task _TCPlistener;


        private CancellationTokenSource _cts;

        public MainWindow()
        {
            Console.WriteLine("Entered");
            InitializeComponent();

            //Carica oggett nella listview della finestra (Esempio di acquisizione oggetti da linea di comando)
            string[] args = Environment.GetCommandLineArgs();
            //Se ci sono file da inviare
            if (args.Length > 1)
            {
                //TODO vai a pagina invia file e chiudi questa (non caricare la trayicon quindi)
                for (int i = 1; i < args.Length; i++)
                {
                    List.Items.Add(args[i]);
                }
            }
            //TODO se ci sono altre sessioni del programma aperte esci (magari aprire una messagebox dicendo che il programma sta già girando)
            
        }

        public override void EndInit()
        {
            base.EndInit();
            _trayIcon.Visible = true;
            //Visibilità finestra (MainWindow.xaml)
            Visibility = Visibility.Visible;
        }

        protected override void OnInitialized(EventArgs e)
        {
            Console.WriteLine("Init");
            base.OnInitialized(e);
            Model.Configuration.LoadConfiguration();
            //Crea TrayIcon
            _trayIcon = new System.Windows.Forms.NotifyIcon {
                Icon = new System.Drawing.Icon(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), //Directory where executable is (NEVER NULL)
                        "Media/switch.ico")
                    )
            };
            //TODO Creare menu click tasto destro
            _trayIcon.DoubleClick += ExitApplication;

            _cts = new CancellationTokenSource();

            _comunication = new LanComunication();

            //Crea thread per mandare pacchetti di advertisement
            _UDPadvertiser = Task.Run( async()=> { await _comunication.LAN_Advertise(_cts.Token); });

            _FileTransfer = new TCP_FileTransfer();

            //Crea thread in ascolto richieste di trasferimento file
            _TCPlistener = Task.Run(async () => { await _FileTransfer.TransferRequestListener(_cts.Token); });
        }

        private void ExitApplication(object sender, EventArgs args)
        {
            _trayIcon.Visible = false;
            _cts.Cancel();
            _UDPadvertiser.Wait();
            _TCPlistener.Wait();
            Application.Current.Shutdown();
        }
        
    }
}
