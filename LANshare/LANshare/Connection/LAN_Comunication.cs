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

namespace LANshare.Connection
{
    class LAN_Comunication
    {

        public async void LAN_Advertise(CancellationToken ct)
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
        }

        public async void LAN_Listen()
        {
        }
    }

}
