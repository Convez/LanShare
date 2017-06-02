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
        /// Continuously sends UDP dgrams to advertise user presence and TCP transmission parameters
        /// </summary>
        /// <param name="ct">Used to stop the advertising during application shutdown</param>
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


            while (!ct.IsCancellationRequested)
            {
                await advertiser.SendAsync(data, data.Length,endpoint);
                Thread.Sleep(Configuration.UdpPacketsIntervalMilliseconds);
            }
            advertiser.DropMulticastGroup(Configuration.MulticastAddress);
        }

        /// <summary>
        /// <para>Listens for user advertisement and saves them into the dictionary for a certain amount of time.</para>
        /// <para>For use before sending files</para>
        /// </summary>
        /// <param name="ct">Used to stop the advertising during application shutdown</param>
        public void LAN_Listen(CancellationToken ct)
        {
            UdpClient listener = new UdpClient(Configuration.UdpPort);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            listener.JoinMulticastGroup(Configuration.MulticastAddress);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var timeout = TimeSpan.FromMilliseconds(Configuration.UserValidityMilliseconds);
            while (!ct.IsCancellationRequested)
            {
                var asyncResult = listener.BeginReceive(null, null);
                asyncResult.AsyncWaitHandle.WaitOne(timeout);
                if (asyncResult.IsCompleted)
                {
                    try
                    {
                        byte[] udpResult = listener.EndReceive(asyncResult, ref endPoint);
                        if (endPoint.Address.Equals(Configuration.CurrentUser.userAddress))
                            continue;
                        using (MemoryStream ms = new MemoryStream(udpResult))
                        {
                            var user = (User)formatter.Deserialize(ms);
                            user.userAddress = IPAddress.Parse(endPoint.Address.ToString());
                            Timer t = new Timer(UserExpired, user, 0, Configuration.UserValidityMilliseconds);
                            UsersOnNetwork.AddOrUpdate(user, t, (u, old) =>
                            {
                                old.Dispose();
                                return t;
                            });
                        }
                    }
                    catch (AggregateException)
                    {
                    }
                }
            }
            listener.DropMulticastGroup(Configuration.MulticastAddress);
        }

        /// <summary>
        /// Removes user from dictionary upon timer expiring
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
