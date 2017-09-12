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
        private Task _UDPlistener;

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
                    //Dimostrazione acquisizione elementi selezionati dalla lista (Può essere tramite doppio click o quando si clicca un bottone)
                    List.MouseDoubleClick += (sender, arg) =>
                    {
                        foreach (string s in List.SelectedItems)
                        {
                            Console.WriteLine(s);
                        }
                    };                    
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
            //Aggiorno listview quando avviene l'evento
            _comunication.UserFound += (sender, args) =>
            {
                string s = args.NickName == ""
                    ? args.Name + "("+args.userAddress.ToString()+")"
                    : args.NickName + "("+args.userAddress.ToString()+")";
                //Usare il dispatcher per eseguire l'aggiornamento dell'interfaccia
                //(Provare ad aggiornare gli items in un thread che non sia il proprietario fa esplodere il programma)
                List.Dispatcher.Invoke(new Action(delegate()
                {
                    if(!List.Items.Contains(s))
                        List.Items.Add(s);
                }));
            };
            //Aggiorno listview quando avviene l'evento
            _comunication.UserExpired += (sender, args) =>
            {
                string s = args.NickName == ""
                    ? args.Name + "(" + args.userAddress.ToString() + ")"
                    : args.NickName + "(" + args.userAddress.ToString() + ")";
                List.Dispatcher.Invoke(new Action(delegate()
                {
                    if (List.Items.Contains(s))
                        List.Items.Remove(s);
                }));
            };
            //Crea thread per mandare pacchetti di advertisement
            _UDPadvertiser = Task.Run( async()=> { await _comunication.LAN_Advertise(_cts.Token); });
            _UDPlistener = Task.Run(async () => { await Task.Run(() => { _comunication.LAN_Listen(_cts.Token); }); });

            _FileTransfer = new TCP_FileTransfer();

            //Crea thread in ascolto richieste di trasferimento file
            _TCPlistener = Task.Run(async () => { await _FileTransfer.TransferRequestListener(_cts.Token); });
        }

        private void ExitApplication(object sender, EventArgs args)
        {
            _trayIcon.Visible = false;
            _cts.Cancel();
            _UDPlistener.Wait();
            _UDPadvertiser.Wait();
            _TCPlistener.Wait();
            Application.Current.Shutdown();
        }
        
    }
}
