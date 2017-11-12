using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using LANshare.Model;
<<<<<<< HEAD
=======
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
>>>>>>> Enrico_TB

namespace LANshare.Connection
{
    class LanComunication
    {
<<<<<<< HEAD
        public ConcurrentDictionary<User, Timer> UsersOnNetwork;
        
        public LanComunication()
        {
            UsersOnNetwork = new ConcurrentDictionary<User, Timer>();
        }
=======
        private CancellationTokenSource cts;
        private Task advertiserTask;
        private Task listenerTask;

        private ExpiringDictionary<User> userList;

        private List<UdpClient> advertisers;
        private List<UdpClient> listeners;

        private int _newSessionIdAvailable;
        private int newSessionIdAvailable
        {
            get => _newSessionIdAvailable;
            set => Interlocked.Exchange(ref _newSessionIdAvailable, value);

        }

        private int numNotified;
        
        /// <summary>
        /// Called when a user is found. Provides the new user as argument.
        /// </summary>
        public event EventHandler<User> UserFound;

        /// <summary>
        /// Called when at least one user is expired. Provides as argument the new list of valid users
        /// </summary>
        public event EventHandler<List<User>> UsersExpired;

        public LanComunication()
        {
            cts = new CancellationTokenSource();
            userList = new ExpiringDictionary<User>(Configuration.UserValidityMilliseconds);
            userList.ElementsExpired += (sender, args) => OnUsersExpired(args);
            newSessionIdAvailable = 0;
            numNotified = 0;
        }

        private List<UdpClient> GenerateUdpClients(int udpPort)
        {
            List<UdpClient> clients = new List<UdpClient>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProperties = nic.GetIPProperties();
                if (!ipProperties.MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped
                if (!nic.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection
                if (OperationalStatus.Up != nic.OperationalStatus)
                    continue; // this adapter is off or not connected
                IPv4InterfaceProperties p = ipProperties.GetIPv4Properties();
                if (p == null)
                    continue; // IPv4 is not configured on this adapter
                ipProperties.UnicastAddresses.ToList().ForEach(
                    (x) =>
                    {
                        if (x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !x.Address.Equals(IPAddress.Loopback))
                        {
                            var edp = new IPEndPoint(x.Address, udpPort); //0 => Assign random free port from the dynamic section (49152-65535)
                            UdpClient cl = new UdpClient();
                            cl.MulticastLoopback = false;
                            int ttl = (int)cl.Client.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive);
                            ttl += 5;
                            cl.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
                            cl.ExclusiveAddressUse = false;
                            cl.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            cl.ExclusiveAddressUse = false;
                            cl.Client.SetSocketOption(SocketOptionLevel.IP,
                                SocketOptionName.AddMembership, new MulticastOption(Configuration.MulticastAddress,x.Address));
                            cl.Client.Bind(edp);
                            clients.Add(cl);
                        }
                    }
                );
            }
            return clients;
        }
        


>>>>>>> Enrico_TB

        /// <summary>
        /// Invia in continuazione datagram UDP per avvisare della propria presenza.
        /// All'interno del pacchetto sono scritto i parametri da usare per la connessione TCP
        /// </summary>
        /// <param name="ct">Usato per fermare l'invio dei pacchetti quando l'applicazione si chiude</param>
<<<<<<< HEAD
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
                Thread.Sleep(1000);
            }
            advertiser.DropMulticastGroup(Configuration.MulticastAddress);
        }

=======
        public void StartLanAdvertise()
        {
            var endpoint = new IPEndPoint(Configuration.MulticastAddress, Configuration.UdpPort);
            List<UdpClient> advertisers = GenerateUdpClients(0);
            this.advertisers = advertisers;

            //Serialize user
            ConnectionMessage userMessage =
                new ConnectionMessage(MessageType.UserAdvertisement, false, Configuration.CurrentUser);
            byte[] data = ConnectionMessage.Serialize(userMessage);
            advertiserTask = Task.Run(()=>advertisers.AsParallel().ForAll((advertiser) =>
            {
                CancellationToken ct = cts.Token;
                int newSessionIdNotified=0;
                while (!ct.IsCancellationRequested && (advertiser.Send(data, data.Length, endpoint)) > 0)
                {
                    Thread.Sleep(Configuration.UdpPacketsIntervalMilliseconds);
                    if (newSessionIdAvailable == 1)
                    {
                        if (newSessionIdNotified == 0)
                        {
                            userMessage.Message = Configuration.CurrentUser;
                            data = ConnectionMessage.Serialize(userMessage);
                            newSessionIdNotified = 1;
                            Interlocked.Increment(ref numNotified);
                            if (numNotified == advertisers.Count)
                            {
                                newSessionIdAvailable = 0;
                            }
                        }
                    }
                    if (newSessionIdAvailable == 0 && newSessionIdNotified == 1)
                    {
                        newSessionIdNotified = 0;
                    }
                }
                try
                {
                    advertiser.DropMulticastGroup(Configuration.MulticastAddress);
                }
                catch (ObjectDisposedException ex)
                {
                    //Object disposed as expected
                }
            }));
        }
        
>>>>>>> Enrico_TB
        /// <summary>
        /// <para>Riceve i pacchetti di annuncio e inserisce gli utenti nel dizionario con un timer</para>
        /// <para>Da usare in fase di invio file</para>
        /// </summary>
        /// <param name="ct">Usato per fermare la ricezione se l'operazione d'invio viene cancellata</param>
<<<<<<< HEAD
        public void LAN_Listen(CancellationToken ct)
        {

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Configuration.UdpPort);
            UdpClient listener = new UdpClient(Configuration.UdpPort);

            listener.JoinMulticastGroup(Configuration.MulticastAddress);
            
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            var timeout = TimeSpan.FromMilliseconds(Configuration.UserValidityMilliseconds);
            byte[] b = {0};
            endPoint.Address = Configuration.MulticastAddress;
            //Soluzione temporanea e poco duratura al timeout dell'igmp snooping nei bridge
            //TODO trovare una soluzione definitiva (attviare applicazione con trayicon se non è attiva?)
            listener.Send(b, 0, endPoint);
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
                            Timer t = new Timer(OnUserExpired, user, Configuration.UserValidityMilliseconds,Timeout.Infinite);
                            //Aggiungi l'utente alla lista. Se l'utente è già stato inserito resetta il timer
                            
                            UsersOnNetwork.AddOrUpdate(user, t, (u, old) =>
                            {
                                old.Dispose();
                                return t;
                            });
                            UserFound(this, user);
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
        private void OnUserExpired(Object o)
        {
            var u = (User)o;
            Timer t;
            UsersOnNetwork.TryRemove(u,out t);
            EventHandler<User> handler = UserExpired;
=======
        public void StartLanListen()
        {
            List<UdpClient> listeners = GenerateUdpClients(Configuration.UdpPort);
            this.listeners = listeners;
            CancellationToken ct = cts.Token;

            listenerTask = Task.Run(() =>
            {
                listeners.AsParallel().ForAll((listener) =>
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            endPoint.Address = IPAddress.Any;
                            endPoint.Port = 0;
                            byte[] udpResult = listener.Receive(ref endPoint);
                            if (!endPoint.Address.Equals(((IPEndPoint) listener.Client.LocalEndPoint).Address))
                            {
                                ConnectionMessage message = ConnectionMessage.Deserialize(udpResult);
                                switch (message?.MessageType)
                                {
                                    case MessageType.UserAdvertisement:
                                        User u = message.Message as User;
                                        u.userAddress = endPoint.Address;
                                        if(userList.Add(u.SessionId,u)) 
                                            OnUserFound(u);
                                        if (u.SessionId.Equals(Configuration.CurrentUser.SessionId))
                                        {
                                            if (newSessionIdAvailable == 0)
                                            {
                                                Configuration.CurrentUser.SessionId = User.GenerateSessionId();
                                                Interlocked.Exchange(ref numNotified, 0);
                                                newSessionIdAvailable = 1;
                                            }
                                        }
                                        break;
                                    case MessageType.UserDisconnectingNotification:
                                        User us = message.Message as User;
                                        userList.Remove(us.SessionId);
                                        break;
                                    //Ignore udp packets where MessageType is not what expected. Might be from different versions of the program
                                }
                            }
                        }
                        catch (SocketException ex)
                        {
                            //As intended
                        }
                        catch (ObjectDisposedException ex)
                        {
                            //As intended
                        }
                    }
                    try
                    {
                        listener.DropMulticastGroup(Configuration.MulticastAddress);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        //Disposed as intended
                    }

                });
            });
            
        }
        public void StopAll()
        {
            cts?.Cancel();
            advertisers?.ForEach(x =>
            {
                ConnectionMessage userMessage = new ConnectionMessage(MessageType.UserDisconnectingNotification, false,
                    Configuration.CurrentUser);
                var data = ConnectionMessage.Serialize(userMessage);
                var endpoint = new IPEndPoint(Configuration.MulticastAddress, Configuration.UdpPort);
                x.Send(data, data.Length, endpoint);
                x.Close();
            });
            listeners?.ForEach(x => x.Close());
            advertiserTask?.Wait();
            listenerTask?.Wait();
        }

        public List<User> GetUsers()
        {
            return userList.GetAll();
        }

        protected void OnUserFound(User u)
        {
            EventHandler<User> handler = UserFound;
>>>>>>> Enrico_TB
            if (handler != null)
            {
                handler(this, u);
            }
        }

<<<<<<< HEAD
        protected void OnUserFound(User u)
        {
            EventHandler<User> handler = UserFound;
            if (handler != null)
            {
                handler(this, u);
            }
        }

        public event EventHandler<User> UserFound;
        public event EventHandler<User> UserExpired;
=======
        protected void OnUsersExpired(List<User> expired)
        {
            EventHandler<List<User>> handler = UsersExpired;
            if (handler != null)
            {
                handler(this, expired);
            }
        }

>>>>>>> Enrico_TB
    }

}
