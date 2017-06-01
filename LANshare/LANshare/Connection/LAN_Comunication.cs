using System;
using System.Linq;
using System.Text;
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
    class LAN_Comunication
    {
        public ConcurrentDictionary<User, Timer> usersOnNetwork;
        public LAN_Comunication()
        {
            usersOnNetwork = new ConcurrentDictionary<User, Timer>();
            
        }
        public async Task LAN_Advertise(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var endpoint = new IPEndPoint(Model.Configuration.MulticastAddress, Model.Configuration.UdpPort);
            UdpClient advertiser = new UdpClient(AddressFamily.InterNetwork);
            advertiser.JoinMulticastGroup(Model.Configuration.MulticastAddress);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, Model.Configuration.CurrentUser);
            byte[] data = ms.ToArray();
            while (!ct.IsCancellationRequested)
            {
                await advertiser.SendAsync(data, data.Length,endpoint);
                Thread.Sleep(10000);
            }
            advertiser.DropMulticastGroup(Configuration.MulticastAddress);
        }
        public async Task LAN_Listen(CancellationToken ct)
        {
            UdpClient listener = new UdpClient(Model.Configuration.UdpPort);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            listener.JoinMulticastGroup(Model.Configuration.MulticastAddress);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var timeout = TimeSpan.FromSeconds(12);
            while (!ct.IsCancellationRequested)
            {
                var asyncResult =  listener.BeginReceive(null, null);
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
                            var user = (Model.User)formatter.Deserialize(ms);
                            user.userAddress = IPAddress.Parse(endPoint.Address.ToString());
                            Timer t = new Timer(ObjectExpired, user, 0, 5000);
                            usersOnNetwork.AddOrUpdate(user, t, (u, old) =>
                            {
                                old.Dispose();
                                return t;
                            });
                        }
                    }
                    catch (AggregateException e)
                    {
                    }
                }
            }
            listener.DropMulticastGroup(Configuration.MulticastAddress);
        }
        private void ObjectExpired(Object o)
        {
            var u = (User)o;
            Timer t;
            usersOnNetwork.TryRemove(u,out t);
        }
    }

}
