using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using LANshare.Model;

namespace LANshare.Connection
{
    class LanComunication
    {
        public ConcurrentDictionary<User, Timer> UsersOnNetwork;
        
        public LanComunication()
        {
            UsersOnNetwork = new ConcurrentDictionary<User, Timer>();
        }

        /// <summary>
        /// Invia in continuazione datagram UDP per avvisare della propria presenza.
        /// All'interno del pacchetto sono scritto i parametri da usare per la connessione TCP
        /// </summary>
        /// <param name="ct">Usato per fermare l'invio dei pacchetti quando l'applicazione si chiude</param>
        public async Task LAN_Advertise(CancellationToken ct)
        {
            var endpoint = new IPEndPoint(Configuration.MulticastAddress, Configuration.UdpPort);
            UdpClient advertiser = new UdpClient(AddressFamily.InterNetwork);
            advertiser.JoinMulticastGroup(Configuration.MulticastAddress);

            //Serialize user
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, Configuration.CurrentUser);
            byte[] data = ms.ToArray();

            // Finche non viene richiesta la cancellazione
            while (!ct.IsCancellationRequested)
            {
                //Manda i pacchetti a certi intervalli
                await advertiser.SendAsync(data, data.Length,endpoint);
                Thread.Sleep(Configuration.UdpPacketsIntervalMilliseconds);
            }
            advertiser.DropMulticastGroup(Configuration.MulticastAddress);
        }

        /// <summary>
        /// <para>Riceve i pacchetti di annuncio e inserisce gli utenti nel dizionario con un timer</para>
        /// <para>Da usare in fase di invio file</para>
        /// </summary>
        /// <param name="ct">Usato per fermare la ricezione se l'operazione d'invio viene cancellata</param>
        public void LAN_Listen(CancellationToken ct)
        {
            UdpClient listener = new UdpClient(Configuration.UdpPort);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            listener.JoinMulticastGroup(Configuration.MulticastAddress);
            
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            var timeout = TimeSpan.FromMilliseconds(Configuration.UserValidityMilliseconds);

            while (!ct.IsCancellationRequested)
            {
                //Inizia la ricezione dei file
                var asyncResult = listener.BeginReceive(null,null);
                asyncResult.AsyncWaitHandle.WaitOne(timeout);
                if (asyncResult.IsCompleted)
                {
                    try
                    {
                        //Ricevi il pacchetto
                        byte[] udpResult = listener.EndReceive(asyncResult, ref endPoint);
                        //Escludi te stesso
                        if (endPoint.Address.Equals(Configuration.CurrentUser.userAddress))
                            continue;
                        using (MemoryStream ms = new MemoryStream(udpResult))
                        {
                            var user = (User)formatter.Deserialize(ms);
                            user.userAddress = endPoint.Address;
                            Timer t = new Timer(UserExpired, user, 0, Configuration.UserValidityMilliseconds);
                            //Aggiungi l'utente alla lista. Se l'utente è già stato inserito resetta il timer
                            UsersOnNetwork.AddOrUpdate(user, t, (u, old) =>
                            {
                                old.Dispose();
                                return t;
                            });
                        }
                    }
                    catch (AggregateException)
                    {
                        //Se il pacchetto è stato ricevuto male (quindi EndReceive o la deserializzazione lancerà delle eccezioni) ignoralo e passa avanti
                    }
                }
            }
            listener.DropMulticastGroup(Configuration.MulticastAddress);
        }

        /// <summary>
        /// Quando il timer scatta rimuove l'utente dal dizionario
        /// </summary>
        /// <param name="o">User to remove</param>
        private void UserExpired(Object o)
        {
            var u = (User)o;
            Timer t;
            UsersOnNetwork.TryRemove(u,out t);
        }
    }

}
