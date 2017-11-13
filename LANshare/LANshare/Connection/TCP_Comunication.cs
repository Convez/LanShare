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
    
    class TCP_Comunication
    {
        private CancellationTokenSource shuttingDown;

        private Task serverTask;

        private List<TcpListener> listeners;

        public event EventHandler<List<string>> fileSendRequested;

        public TCP_Comunication()
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
                            server.Start(50);
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
            ConnectionMessage message = await ReadMessage(client.Client);
            switch (message.MessageType)
            {
                case MessageType.IpcBaseFolder:
                    List<string> files = new List<string>();
                    do
                    {
                        files.Add(message.Message as string);
                        message = await ReadMessage(client.Client);
                    } while (message.Next);
                    files.Add(message.Message as string);
                    OnSendRequested(files);
                    break;
            }
                
        }

        private void OnSendRequested(List<string> l)
        {
            fileSendRequested?.Invoke(this, l);
        }

        private async Task<ConnectionMessage> ReadMessage(Socket from)
        {
            byte[] messageSize = new byte[sizeof(long)];
            int bytesRed = from.Receive(messageSize, messageSize.Length, SocketFlags.None);
            if (bytesRed < 0)
            {
                MessageBox.Show("Connection aborted");
                return null;
            }
            long messageLength = BitConverter.ToInt64(messageSize, 0);
            byte[] readVector = new byte[IPAddress.NetworkToHostOrder(messageLength)];
            bytesRed = from.Receive(readVector, readVector.Length, SocketFlags.None);
            return bytesRed<=0?null:ConnectionMessage.Deserialize(readVector);
        }

        private async Task SendMessage(Socket to, ConnectionMessage message)
        {
            byte[] data = ConnectionMessage.Serialize(message);
            byte[] dataSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.LongLength));
            int l = sizeof(long);
           
            to.Send(dataSize, dataSize.Length, SocketFlags.None);
            to.Send(data, data.Length, SocketFlags.None);
        }

        public bool SendFileList(string[] files)
        {
            try
            {
                TcpClient client = new TcpClient(IPAddress.Loopback.ToString(), Configuration.TcpPort);
                ConnectionMessage message = new ConnectionMessage(MessageType.IpcBaseFolder, true, files[0]);
                SendMessage(client.Client,message).Wait();
                message.MessageType = MessageType.IpcElement;
                for (int i = 1; i < files.Length - 1; i++)
                {
                    message.Message = files[i];
                    SendMessage(client.Client, message).Wait();
                }
                message.Message = files[files.Length - 1];
                message.Next = false;
                SendMessage(client.Client, message).Wait();

                client.Close();
                return true;
            }
            catch (SocketException)
            {
                //Most likely failed to connect
                return false;
            }
        }


        public static bool OtherInstanceRunning()
        {
            try
            {
                TcpListener l = new TcpListener(new IPEndPoint(IPAddress.Loopback, Configuration.TcpPort));
                l.Start();
                l.Stop();
                return false;
            }
            catch (SocketException)
            {
                return true;
            }
        }

    }
}
