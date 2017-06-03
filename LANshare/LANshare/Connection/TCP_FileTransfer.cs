using LANshare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LANshare.Connection
{
    [Serializable]
    public enum EFileRequestState
    {
        Get,
        Accepted,
        NotAccepted,
        Err
    }
    class TCP_FileTransfer
    {
        public async Task TransferRequestListener(CancellationToken ct)
        {

            Socket listSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, Configuration.TcpPort);
            listSock.Bind(endpoint);
            listSock.Listen(20);
            while(!ct.IsCancellationRequested)
            {
                var cliSock = listSock.Accept();
                
                if (/*!((System.Net.IPEndPoint) cliSock.RemoteEndPoint).Address.Equals(Configuration.CurrentUser
                    .userAddress)*/true)
                {
                    Task.Run(async () => await Task.Run(() => ServeClient(cliSock, ct)));
                }
                else
                {
                    CloseSocket(cliSock);
                }
                
            }
            listSock.Shutdown(SocketShutdown.Both);
            listSock.Disconnect(false);
            listSock.Close();
            listSock.Dispose();
        }

        private void ServeClient(Socket cliSock, CancellationToken ct)
        {
            byte[] data = new byte[cliSock.ReceiveBufferSize];
            cliSock.Receive(data);
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var request = (EFileRequestState) formatter.Deserialize(ms);
            if (request != EFileRequestState.Get)
            {
                request = EFileRequestState.Err;
                ms.Flush();
                formatter.Serialize(ms, request);
                ms.Read(data, 0, data.Length);
                cliSock.Send(data);
            }
            else
            {
                request = EFileRequestState.Accepted;
                ms.Seek(0, SeekOrigin.Begin);
                formatter.Serialize(ms, request);
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(data, 0, data.Length);
                cliSock.Send(data);
                //TODO start receiving files
            }
            CloseSocket(cliSock);
        }
        public async Task TransferRequestSender(User server, List<string> filesToSend)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new System.Net.IPEndPoint(server.userAddress, server.TcpPortTo);
            client.Connect(endpoint);
            var data = new byte[client.ReceiveBufferSize];
            EFileRequestState request = EFileRequestState.Get;
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, request);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(data, 0, data.Length);
            try
            {
                client.Send(data);
                client.Receive(data);
                if (data.Length > 0)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    request = (EFileRequestState) formatter.Deserialize(ms);
                    if (request == EFileRequestState.Err || request == EFileRequestState.NotAccepted)
                    {
                        CloseSocket(client);
                    }
                    else
                    {
                        //TODO start sending files
                    }

                }
                else
                {
                    CloseSocket(client);
                }
            }
            catch (AggregateException)
            {
                
            }
        }

        private void CloseSocket(Socket client)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(false);
            client.Close();
        }
        
    }
}
