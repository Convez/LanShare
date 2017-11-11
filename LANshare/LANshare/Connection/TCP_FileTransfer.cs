using LANshare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LANshare.Connection
{
    
    class TCP_FileTransfer
    {
        private CancellationTokenSource shuttingDown;

        private Task serverTask;

        private List<TcpListener> listeners;

        public TCP_FileTransfer()
        {
            shuttingDown = new CancellationTokenSource();
        }

        private List<TcpListener> GenerateServers(int tcpPort)
        {
            List<TcpListener> servers = new List<TcpListener>();
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
                    (addr) =>
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            var edp = new IPEndPoint(addr.Address, tcpPort);
                            TcpListener server = new TcpListener(edp);
                            servers.Add(server);
                        }
                    }
                 );
            }
            return servers;
        }

        public void StartTcpServers()
        {
            listeners = GenerateServers(Configuration.TcpPort);
            serverTask = Task.Run(() => listeners.AsParallel().ForAll((server) =>
            {
                CancellationToken shutDown = shuttingDown.Token;
                try
                {
                    server.Start(50);
                }
                catch (SocketException ex)
                {
                    MessageBox.Show("Can't start server.\n" + ex.Message);
                    return;
                }
                while(!shutDown.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient clientAccepted = server.AcceptTcpClient();
                        Task.Run(async () => await HandleClient(clientAccepted,shutDown));
                    }
                    catch (SocketException)
                    {

                    }
                    catch (ObjectDisposedException)
                    {
                        
                    }
                }
            }));
        }

        private async Task HandleClient(TcpClient client, CancellationToken ct)
        {
            using (NetworkStream ns = client.GetStream())
            {
                while (!ct.IsCancellationRequested)
                {
                    ns.ReadTimeout = Configuration.TCPConnectionTimeoutMilliseconds;
                    ConnectionMessage message = await ReadMessage(ns);
                }
            }
        }

        private async Task<ConnectionMessage> ReadMessage(NetworkStream from)
        {
            byte[] messageSize = new byte[sizeof(long)];
            int bytesRed = await from.ReadAsync(messageSize,0,messageSize.Length);
            if (bytesRed < 0)
            {
                MessageBox.Show("Connection aborted");
                return null;
            }
            long messageLength = BitConverter.ToInt64(messageSize, 0);
            byte[] readVector = new byte[IPAddress.NetworkToHostOrder(messageLength)];
            bytesRed = await from.ReadAsync(readVector, 0, readVector.Length);
            return ConnectionMessage.Deserialize(readVector);
        }

        private async Task SendMessage(NetworkStream to, ConnectionMessage message)
        {
            using (MemoryStream ss = new MemoryStream())
            {
                byte[] data = ConnectionMessage.Serialize(message);
                byte[] dataSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.LongLength));
                await to.WriteAsync(dataSize, 0, data.Length);
                await to.WriteAsync(data, 0, data.Length);
                await to.FlushAsync();
            }
        }
        
    }
}
