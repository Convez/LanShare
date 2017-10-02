using LANshare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        /// <summary>
        /// Rimane in ascolto per eventuali richeste di trasferimento file
        /// </summary>
        /// <param name="ct">Usato per fermare l'accettazione di nuove connessioni quando l'applicazione si chiude</param>
        /// <returns></returns>
        public async Task TransferRequestListener(CancellationToken ct)
        {
            var timeout = TimeSpan.FromMilliseconds(Configuration.TCPConnectionTimeoutMilliseconds);
            System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, Configuration.TcpPort);
            TcpListener listener = new TcpListener(endpoint); ;
            try
            {
                listener.Start(40);
            }
            catch (SocketException e)
            {
                MessageBox.Show("Can't start server. Port occupied.");
                return;
            }
            while (!ct.IsCancellationRequested)
            {
                var acceptClientResult = listener.BeginAcceptTcpClient(null, null);
                acceptClientResult.AsyncWaitHandle.WaitOne(timeout);
                if (acceptClientResult.IsCompleted)
                {
                    try
                    {
                        TcpClient currentClient = listener.EndAcceptTcpClient(acceptClientResult);
                        //Escludi te stesso
                        if (!((IPEndPoint) currentClient.Client.RemoteEndPoint).Address.Equals(Configuration.CurrentUser
                            .userAddress))
                        {
                            Task.Run(async () =>
                             {
                                 await Task.Run(() => { ServeClient(currentClient, ct); });
                             });
                        }
                        else
                        {
                            currentClient.Close();
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        //Error connecting to the client. go on
                    }
                }
            }
            listener.Stop();
        }

        /// <summary>
        /// Serve il client appena connesso
        /// </summary>
        /// <param name="cliSock">Socket del client</param>
        /// <param name="ct"></param>
        private void ServeClient(TcpClient cliSock, CancellationToken ct)
        {
            
            using (NetworkStream ns = cliSock.GetStream())
            {
                ns.ReadTimeout = Configuration.TCPConnectionTimeoutMilliseconds;

                //Get size of serialized counterpart data
                byte[] sizeBuf = new byte[sizeof(int)];
                int bytesRed = ns.Read(sizeBuf, 0, sizeBuf.Length);
                if (bytesRed < 0) {
                    MessageBox.Show("Cannot receive data from counterpart. Please check connection to the LAN and try again");
                    return;
                }
                int size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(sizeBuf, 0));

                //Get client parameters (Identity and security)
                byte[] formattedUser = new byte[size];
                ns.Read(formattedUser, 0, formattedUser.Length);

                IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                User counterpart;
                using (MemoryStream ms = new MemoryStream(formattedUser)) {
                    counterpart = (User)formatter.Deserialize(ms);
                }
                

                //TODO Get number of elements
                byte[] nElemBuf = new byte[sizeof(int)];
                bytesRed = ns.Read(nElemBuf, 0, nElemBuf.Length);
                if (bytesRed < 0) {
                    MessageBox.Show("Cannot receive data from user " + counterpart.NickName!=""?counterpart.NickName:counterpart.Name + ". Please check connection to the LAN and try again");
                    return;
                }
                int nElem = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(nElemBuf,0));

                //TODO Ask user for confermation if necessary
                MessageBox.Show("User " + counterpart.NickName != "" ? counterpart.NickName : counterpart.Name + " wants to send " + nElem.ToString() + " files/folders. Accept?");
                
                //TODO For each element get type, fileNameSize, fileName and transfer it


            }
        }

        /// <summary>
        /// Connette ad un host remoto e manda i file
        /// </summary>
        /// <param name="server">Dati dell'utente a cui connettersi</param>
        /// <param name="filesToSend">Lista dei file da mandare</param>
        /// <param name="ct">Usato per fermare la trasmissione dei file</param>
        /// <returns></returns>
        public async Task TransferRequestSender(User server, List<string> filesToSend, CancellationToken ct)
        {
            try
            {
                TcpClient client = new TcpClient(server.userAddress.ToString(), server.TcpPortTo);
                using (NetworkStream ns = client.GetStream())
                {
                    //TODO Send Info

                    //Serialize user
                    IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    formatter.Serialize(ms, Configuration.CurrentUser);
                    byte[] data = ms.ToArray();
                    //Send size of serialized data
                    int sizeFormatted = IPAddress.HostToNetworkOrder(data.Length);
                    byte[] sizeBuf = BitConverter.GetBytes(sizeFormatted);
                    ns.Write(sizeBuf, 0, sizeBuf.Length);
                    //Send serialized data
                    ns.Write(data, 0, data.Length);

                    //TODO Send number of elements to send
                    int numFiles = IPAddress.HostToNetworkOrder(filesToSend.Count);
                    byte[] nElemBuf = BitConverter.GetBytes(numFiles);
                    ns.Write(nElemBuf, 0, nElemBuf.Length);
                    Console.WriteLine("Sent "+ filesToSend.Count.ToString());
                    //TODO Wait for ok




                    //TODO For each element send type, fileNameSize, fileName and transfer it

                }
            }
            catch (SocketException e)
            {
                //Error connecting to client
                //TODO Print error message
            }
        
        }
        
        
    }
}
