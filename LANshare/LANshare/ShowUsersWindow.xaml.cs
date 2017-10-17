using LANshare.Connection;
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
using System.ComponentModel;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for ShowUsersWindow.xaml
    /// </summary>
    public partial class ShowUsersWindow : Window
    {
        private LanComunication _comunication;
        private Task _UDPlistener;
        private CancellationTokenSource _cts;
        private TrayIconWindow trayIconWindow;

        public ShowUsersWindow()
        {
            InitializeComponent();
            
        }

        public ShowUsersWindow(TrayIconWindow trayIconWindow)
        {
            this.trayIconWindow = trayIconWindow;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _cts = new CancellationTokenSource();
            _comunication = new LanComunication();
            //Aggiorno listview quando avviene l'evento
            _comunication.UserFound += (sender, args) =>
            {
                string s = args.NickName == ""
                    ? args.Name + "(" + args.userAddress.ToString() + ")"
                    : args.NickName + "(" + args.userAddress.ToString() + ")";
                //Usare il dispatcher per eseguire l'aggiornamento dell'interfaccia
                //(Provare ad aggiornare gli items in un thread che non sia il proprietario fa esplodere il programma)
                ConnectedUsers.Dispatcher.Invoke(new Action(delegate ()
                {
                    if (!ConnectedUsers.Items.Contains(s))
                        ConnectedUsers.Items.Add(s);
                }));
            };
            //Aggiorno listview quando avviene l'evento
            _comunication.UserExpired += (sender, args) =>
            {
                string s = args.NickName == ""
                    ? args.Name + "(" + args.userAddress.ToString() + ")"
                    : args.NickName + "(" + args.userAddress.ToString() + ")";
                ConnectedUsers.Dispatcher.Invoke(new Action(delegate ()
                {
                    if (ConnectedUsers.Items.Contains(s))
                        ConnectedUsers.Items.Remove(s);
                }));
            };

            _UDPlistener = Task.Run(async () => { await Task.Run(() => { _comunication.LAN_Listen(_cts.Token); }); });
        }
        protected override void OnClosed(EventArgs e)
        {
            _cts.Cancel();
            _UDPlistener.Wait();
            base.OnClosed(e);
            if (trayIconWindow != null)
            {
                trayIconWindow.RestoreItem();
            }
            //Application.Current.Shutdown();
        }
    }
}
