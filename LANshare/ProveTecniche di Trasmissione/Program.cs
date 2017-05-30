using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LANshare.Model;

namespace ProveTecniche_di_Trasmissione
{
    class Program
    {


        static void Main(string[] args)
        {

            var v = new com();
            Task t = new Task(v.LAN_Advertise);
            
            Task<bool> p = new Task<bool>(v.LAN_Listen);
            p.Start();
            t.Start();
            p.Wait();
            t.Wait();
            
        }
    }

    class com
    {
        private readonly System.Net.IPAddress _multicastAddress;
        private readonly User currentUser;
        public com()
        {
            _multicastAddress = System.Net.IPAddress.Parse("239.165.41.74");
            currentUser = new User();
            currentUser.name = "Tonino";
            currentUser.surname = "Accolla";
        }
        public async void LAN_Advertise()
        {
            var endPoint = new System.Net.IPEndPoint(_multicastAddress, 10666);
            var client = new System.Net.Sockets.UdpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            client.JoinMulticastGroup(_multicastAddress);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, currentUser);
            var data = ms.ToArray();
            ms.Dispose();
            while (true)
            {
                await client.SendAsync(data, data.Length, endPoint);
                System.Threading.Thread.Sleep(10000);
            }
        }

        public bool LAN_Listen()
        {
            System.Net.Sockets.UdpClient client = new UdpClient(10666);
            var endPoint = new System.Net.IPEndPoint(IPAddress.Any, 0);
            client.JoinMulticastGroup(_multicastAddress);

            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            while (true)
            {
                var data = client.Receive(ref endPoint);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    User u = (User) formatter.Deserialize(ms);
                    Console.WriteLine(endPoint.Address.ToString() + @" " + u.ToString());
                }
            }
            return true;
        }
    }
}
