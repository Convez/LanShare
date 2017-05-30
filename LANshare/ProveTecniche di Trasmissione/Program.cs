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
using System.Security.Cryptography;

namespace ProveTecniche_di_Trasmissione
{
    class Program
    {


        static void Main(string[] args)
        {

            /*
            var v = new com();
            Task t = new Task(v.LAN_Advertise);
            
            Task<bool> p = new Task<bool>(v.LAN_Listen);
            p.Start();
            t.Start();
            p.Wait();
            t.Wait();
            */

            infoSendRec i = new infoSendRec();
            Task t = new Task(i.InfoServer);
            Task p = new Task(i.InfoClient);
            t.Start();
            p.Start();
            t.Wait();
            p.Wait();
            Console.ReadKey();

        }
    }

    class infoSendRec
    {
        private readonly int _port = 42666;


        public async void InfoServer()
        {
            TcpListener server = new TcpListener(IPAddress.Any, _port);
            server.Start();
            var client = server.AcceptTcpClient();
            var str = client.GetStream();
            byte[] data = new byte[client.ReceiveBufferSize];
            str.Read(data, 0, client.ReceiveBufferSize);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream(data);
            List<string> filesToReceive = (List<string>) formatter.Deserialize(ms);
            filesToReceive.ForEach(Console.WriteLine);
        }

        public async void InfoClient()
        {
            TcpClient client = new TcpClient("127.0.0.1", _port);
            var str =client.GetStream();
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            List<string> filesToSend = new List<string>();
            filesToSend.Add("Giovanni");
            filesToSend.Add("Muciaccia");
            formatter.Serialize(ms,filesToSend);
            byte[] data = ms.ToArray();
            str.Write(data, 0, data.Length);
        }
    }


    class com
    {
        private readonly IPAddress _multicastAddress;
        private readonly int _port;
        private readonly User currentUser;
        public com()
        {
            _port = 44666;
            _multicastAddress = System.Net.IPAddress.Parse("239.165.41.74");
            currentUser = new User();
            currentUser.name = "Tonino";
            currentUser.surname = "Accolla";
        }
        
        public async void LAN_Advertise()
        {
            var endPoint = new IPEndPoint(_multicastAddress, _port);
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.JoinMulticastGroup(_multicastAddress);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, "42666");
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
            UdpClient client = new UdpClient(_port);
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            client.JoinMulticastGroup(_multicastAddress);

            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            while (true)
            {
                var data = client.Receive(ref endPoint);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var u = (string) formatter.Deserialize(ms);
                    Console.WriteLine(endPoint.Address + @" " + u);
                }
            }
            return true;
        }
    }
}
