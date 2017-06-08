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
        Send,
        Accepted,
        NotAccepted,
        Err
    }

    class TCP_FileTransfer
    {

        /// <summary>
        /// Rimane in ascolto per eventuali richeste di trasferimento file
        /// </summary>
        /// <param name="ct">Usato per fermare l'accettazione di nuove connessioni quando l'applicazione si chiude</param>
        /// <returns></returns>
        public async Task TransferRequestListener(CancellationToken ct)
        {
            //Crea socket inizia ad ascoltare per connessioni sulla porta
            Socket listSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            System.Net.IPEndPoint endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, Configuration.TcpPort);
            listSock.Bind(endpoint);
            listSock.Listen(20);

            while(!ct.IsCancellationRequested)
            {
                //Accetta connessione
                var cliSock = listSock.Accept();
                //Escludi te stesso
                if (!((System.Net.IPEndPoint) cliSock.RemoteEndPoint).Address.Equals(Configuration.CurrentUser
                    .userAddress))
                {
                    Task.Run(async () => await Task.Run(() => ServeClient(cliSock, ct)));
                }
                else
                {
                    CloseSocket(cliSock);
                }
                
            }
            CloseSocket(listSock);
        }

        /// <summary>
        /// Serve il client appena connesso
        /// </summary>
        /// <param name="cliSock">Socket del client</param>
        /// <param name="ct"></param>
        private void ServeClient(Socket cliSock, CancellationToken ct)
        {
            byte[] data = new byte[cliSock.ReceiveBufferSize];
            //Get current request state
            cliSock.Receive(data);

            //Deserialize request state
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            var request = (EFileRequestState) formatter.Deserialize(ms);

            //If remote client doesn't want to send
            if (request != EFileRequestState.Send)
            {
                //Send error
                request = EFileRequestState.Err;
                ms.Flush();
                formatter.Serialize(ms, request);
                ms.Read(data, 0, data.Length);
                cliSock.Send(data);
            }
            else
            {
                //TODO ask who is sending 

                //TODO ask user for acceptance

                //Send Accept
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

        /// <summary>
        /// Connette ad un host remoto e manda i file
        /// </summary>
        /// <param name="server">Dati dell'utente a cui connettersi</param>
        /// <param name="filesToSend">Lista dei file da mandare</param>
        /// <param name="ct">Usato per fermare la trasmissione dei file</param>
        /// <returns></returns>
        public async Task TransferRequestSender(User server, List<string> filesToSend, CancellationToken ct)
        {
            //Crea socket e connetti all'utente remoto
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new System.Net.IPEndPoint(server.userAddress, server.TcpPortTo);
            client.Connect(endpoint);
            //Serializza richiesta di mandare file
            var data = new byte[client.ReceiveBufferSize];
            EFileRequestState request = EFileRequestState.Send;
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, request);
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(data, 0, data.Length);

            try
            {
                //Manda richiesta
                client.Send(data);
                //TODO manda chi sei
                //Aspetta risposta
                client.Receive(data);
                //Se sono stati ricevuti dei file (L'host remoto c'è ancora, quindi il socket non è stato chiuso)
                if (data.Length > 0)
                {
                    //Deserializza
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    request = (EFileRequestState) formatter.Deserialize(ms);
                    //Se c'è stato un errore oppure l'utente remoto non accetta i file
                    if (request == EFileRequestState.Err || request == EFileRequestState.NotAccepted)
                    {
                        //Staccah
                        CloseSocket(client);
                    }
                    else
                    {
                        //TODO start sending files
                    }

                }
                else
                {
                    //Staccah
                    CloseSocket(client);
                }
            }
            catch (AggregateException)
            {
                //TODO errore di trasmissione -> Avvisa utente (riprova?)
            }
        }
        
        /// <summary>
        /// Staccah
        /// </summary>
        /// <param name="client">Da staccahre</param>
        private void CloseSocket(Socket client)
        {
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(false);
            client.Close();
            client.Dispose();
        }
        
    }
}
