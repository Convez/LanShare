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
            TcpListener listener = new TcpListener(endpoint);
            listener.Start(40);
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
                //TODO Get number of elements
                byte[] nElemBuf = new byte[sizeof(uint)];
                ns.Read(nElemBuf, 0, nElemBuf.Length);
                UInt32 nElem = BitConverter.ToUInt32(nElemBuf, 0);
                Console.WriteLine("Received " + nElem.ToString());
                //TODO Ask user for confermation if necessary

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

                    //TODO Send number of elements to send
                    byte[] nElemBuf = BitConverter.GetBytes((uint)filesToSend.Count);
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
