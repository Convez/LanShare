﻿using System;
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
            userList.ElementsExpired += (sender, args) => OnUsersExpired(userList.GetAll());
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
        



        /// <summary>
        /// Invia in continuazione datagram UDP per avvisare della propria presenza.
        /// All'interno del pacchetto sono scritto i parametri da usare per la connessione TCP
        /// </summary>
        /// <param name="ct">Usato per fermare l'invio dei pacchetti quando l'applicazione si chiude</param>
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
                while (!ct.IsCancellationRequested && (advertiser.Send(data, data.Length, endpoint)) > 0)
                {
                    Thread.Sleep(Configuration.UdpPacketsIntervalMilliseconds);
                }
                try
                {
                    userMessage = new ConnectionMessage(MessageType.UserDisconnectingNotification, false,
                        Configuration.CurrentUser);
                    data = ConnectionMessage.Serialize(userMessage);
                    //Non me ne frega un cazzo se il pacchetto va perso, tanto chissene c'è il timer
                    advertiser.Send(data,data.Length,endpoint);
                    advertiser.DropMulticastGroup(Configuration.MulticastAddress);
                }
                catch (ObjectDisposedException ex)
                {
                    //Object disposed as expected
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
                                        User u = message.Message as User;
                                        if(userList.Add(u.SessionId,u)) 
                                            OnUserFound(u);
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
            advertisers?.ForEach(x => x.Close());
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
            if (handler != null)
            {
                handler(this, u);
            }
        }

        protected void OnUsersExpired(List<User> newList)
        {
            EventHandler<List<User>> handler = UsersExpired;
            if (handler != null)
            {
                handler(this, newList);
            }
        }

    }

}
