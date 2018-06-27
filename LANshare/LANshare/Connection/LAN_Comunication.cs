using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using LANshare.Model;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using Newtonsoft.Json;

namespace LANshare.Connection
{
    class LanComunication
    {
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
            NetworkChange.NetworkAvailabilityChanged += NetworkStatusChangedCallback;
        }
        ~LanComunication()
        {
            NetworkChange.NetworkAvailabilityChanged -= NetworkStatusChangedCallback;
        }
        private List<UdpClient> GenerateUdpClients(int udpPort)
        {
            List<UdpClient> clients = new List<UdpClient>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProperties = nic.GetIPProperties();
                if (!ipProperties.MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped
                if (OperationalStatus.Up != nic.OperationalStatus)
                    continue; // this adapter is off or not connected
                try{
                IPv4InterfaceProperties p = ipProperties.GetIPv4Properties();
                    if (p == null)
                        continue; // IPv4 is not configured on this adapter
                }catch(NetworkInformationException e){
                    continue;
                }
                ipProperties.UnicastAddresses.ToList().ForEach(
                    (x) =>
                    {
                        if (x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !x.Address.Equals(IPAddress.Loopback))
                        {
                            var edp = new IPEndPoint(x.Address, udpPort); //0 => Assign random free port from the dynamic section (49152-65535)
                            UdpClient cl = new UdpClient();
                            cl.MulticastLoopback = false;
                            int ttl = (int)cl.Client.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive);
                            ttl += 15;
                            cl.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
                            cl.ExclusiveAddressUse = false;
                            cl.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            cl.ExclusiveAddressUse = false;
                            cl.Client.SetSocketOption(SocketOptionLevel.IP,
                                SocketOptionName.AddMembership, new MulticastOption(Configuration.MulticastAddress, x.Address));
                            cl.Client.Bind(edp);
                            clients.Add(cl);
                        }
                    }
                );
            }
            return clients;
        }

        private void NetworkStatusChangedCallback(object sender, EventArgs args)
        {
            StopAll();
            userList.Reset();
            StartLanListen();
            StartLanAdvertise();
        }


        /// <summary>
        /// Invia in continuazione datagram UDP per avvisare della propria presenza.
        /// All'interno del pacchetto sono scritto i parametri da usare per la connessione TCP
        /// </summary>
        /// <param name="ct">Usato per fermare l'invio dei pacchetti quando l'applicazione si chiude</param>
        public void StartLanAdvertise()
        {
            if (Configuration.UserAdvertisementMode == EUserAdvertisementMode.Private)
                return;
            var endpoint = new IPEndPoint(Configuration.MulticastAddress, Configuration.UdpPort);
            List<UdpClient> advertisers = GenerateUdpClients(0);
            this.advertisers = advertisers;
            
            advertiserTask = Task.Run(()=>advertisers.AsParallel().ForAll((advertiser) =>
            {
                CancellationToken ct = cts.Token;
                int newSessionIdNotified=0;
                try
                {
                    //Serialize user

                    ConnectionMessage userMessage =
                        new ConnectionMessage(MessageType.UserAdvertisement, false, Configuration.CurrentUser);
                    byte[] data = ConnectionMessage.Serialize(userMessage);
                    long previousTime = Configuration.CurrentUser.LastPicModification;

                    while (!ct.IsCancellationRequested && (advertiser.Send(data, data.Length, endpoint)) > 0)
                    {
                        if (Configuration.CurrentUser.LastPicModification > previousTime)
                        {
                            userMessage = new ConnectionMessage(MessageType.UserAdvertisement, false, Configuration.CurrentUser);
                            data = ConnectionMessage.Serialize(userMessage);
                            previousTime = Configuration.CurrentUser.LastPicModification;
                        }
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
                }
                catch (ObjectDisposedException ex)
                {
                    //Object disposed as expected
                }
                catch (SocketException ex)
                {
                    //Disposed as intended
                }
            }));
        }
        
        /// <summary>
        /// <para>Riceve i pacchetti di annuncio e inserisce gli utenti nel dizionario con un timer</para>
        /// <para>Da usare in fase di invio file</para>
        /// </summary>
        /// <param name="ct">Usato per fermare la ricezione se l'operazione d'invio viene cancellata</param>
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
                                        User u = JsonConvert.DeserializeObject<User>(message.Message.ToString());
                                        u.UserAddress = endPoint.Address;
                                        if (userList.Add(u.SessionId, u))
                                        {
                                            try
                                            {
                                                u.SetupImage();
                                            }catch(Exception ex)
                                            {

                                            }
                                            OnUserFound(u);
                                        }
                                        else
                                        {
                                            User previousUDP = userList.Get(u.SessionId);
                                            if (previousUDP.LastPicModification < u.LastPicModification)
                                            {
                                                previousUDP.UserAddress = endPoint.Address;
                                                previousUDP.LastPicModification = u.LastPicModification;
                                                previousUDP.SetupImage();
                                            }
                                        }
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
                                        User us = JsonConvert.DeserializeObject<User>(message.Message.ToString());
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

                });
            });
            
        }

        public void GoPrivate()
        {
            advertisers?.ForEach(x =>
            {
                ConnectionMessage userMessage = new ConnectionMessage(MessageType.UserDisconnectingNotification, false,
                    Configuration.CurrentUser);
                var data = ConnectionMessage.Serialize(userMessage);
                var endpoint = new IPEndPoint(Configuration.MulticastAddress, Configuration.UdpPort);
                x.Send(data, data.Length, endpoint);
                x.Close();
            });
            advertiserTask?.Wait();
        }
        public void StopAll()
        {
            cts?.Cancel();
            GoPrivate();
            listeners?.ForEach(x => x.Close());
            listenerTask?.Wait();
        }

        public List<User> GetUsers()
        {
            return userList.GetAll();
        }

        protected void OnUserFound(User u)
        {
            UserFound?.Invoke(this, u);
        }

        protected void OnUsersExpired(List<User> expired)
        {
            UsersExpired?.Invoke(this, expired);
        }

    }

}
