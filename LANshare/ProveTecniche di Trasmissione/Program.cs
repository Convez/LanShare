using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        public com()
        {
            _multicastAddress = System.Net.IPAddress.Parse("239.165.41.74");
        }
        public async void LAN_Advertise()
        {
            var endPoint = new System.Net.IPEndPoint(_multicastAddress, 10666);
            var client = new System.Net.Sockets.UdpClient(System.Net.Sockets.AddressFamily.InterNetwork);
            client.JoinMulticastGroup(_multicastAddress);
            var data = Encoding.Default.GetBytes("Giovanni Muciaccia");
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
            while (true)
            {
                var data = client.Receive(ref endPoint);
                var message = Encoding.Default.GetString(data);
                Console.WriteLine(message);
            }
            return true;
        }
    }
}
