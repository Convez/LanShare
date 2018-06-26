using LANshare.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LANshare.Connection
{
    
    class TCP_Comunication
    {
        private CancellationTokenSource shuttingDown;

        private Task serverTask;
        private readonly object l = "";
        private List<TcpListener> listeners;
        private TcpListener loopback;
        private Task loopbackTask;
        public event EventHandler requestSeen;
        public event EventHandler<List<string>> FileSendRequested;
        public event EventHandler<User> TransferRequested;
        public event EventHandler<IFileTransferHelper> UploadAccepted;
        
        public TCP_Comunication()
        {
            NetworkChange.NetworkAvailabilityChanged += NetAvailabilityCallback;
        }
        ~TCP_Comunication()
        {
            NetworkChange.NetworkAvailabilityChanged -= NetAvailabilityCallback;
        }

        private List<TcpListener> GenerateServers(int tcpPort)
        {
            List<TcpListener> servers = new List<TcpListener>();
            if (loopback == null)
            {
                Debug.WriteLine(loopback);
                loopback = new TcpListener(new IPEndPoint(IPAddress.Loopback, 42666));
                try {
                    loopback.Start(50);
                    loopbackTask = Task.Run(() => 
                    {
                        CancellationToken shutDown = shuttingDown.Token;
                        while (!shutDown.IsCancellationRequested)
                        {
                            try
                            {
                                TcpClient clientAccepted = loopback.AcceptTcpClient();
                                Task.Run(() => HandleClient(clientAccepted, shutDown));
                            }
                            catch (SocketException)
                            {
                            }
                            catch (ObjectDisposedException)
                            {
                            }
                            catch (InvalidOperationException) { }
                        }
                    });
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Magia nera");
                }
            }
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProperties = nic.GetIPProperties();
                if (!ipProperties.MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped
                if (!nic.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection
                if (OperationalStatus.Up != nic.OperationalStatus)
                    continue; // this adapter is off or not connected
                try{
                    IPv4InterfaceProperties p = ipProperties.GetIPv4Properties();
                    if (p == null)
                        continue; // IPv4 is not configured on this adapter
                }catch(NetworkInformationException e){
                    continue;// This adapter does not support IPv4
                }
                ipProperties.UnicastAddresses.ToList().Where(addr=>!addr.Address.Equals(IPAddress.Loopback))
                    .ToList().ForEach(
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
            shuttingDown = new CancellationTokenSource();
            if (Configuration.UserAdvertisementMode == EUserAdvertisementMode.Private)
                return;
            listeners = GenerateServers(Configuration.TcpPort);
            serverTask = Task.Run(() => listeners.AsParallel().ForAll((server) =>
            {
                CancellationToken shutDown = shuttingDown.Token;
                while(!shutDown.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient clientAccepted = server.AcceptTcpClient();
                        Task.Run(() => HandleClient(clientAccepted, shutDown));
                    }
                    catch (SocketException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (InvalidOperationException) { }
                }
            }));
        }
        
        private void NetAvailabilityCallback(object sender, NetworkAvailabilityEventArgs args)
        {
            
            StopAll();
            StartTcpServers();
        }
        
        public void StopAll()
        {
            shuttingDown?.Cancel();
            listeners?.ForEach((l)=> { l.Stop(); });
            serverTask?.Wait();
        }
        public void StopLoopback()
        {
            loopback?.Stop();
            loopbackTask?.Wait();
        }
        public void RequestImage(User from)
        {
            try
            {
                TcpClient client = new TcpClient(from.UserAddress.ToString(), from.TcpPortTo);
                ConnectionMessage message = new ConnectionMessage(MessageType.ProfileImageRequest, false, null);
                SendMessage(client, message);
                message = ReadMessage(client);
                if (message.MessageType == MessageType.ProfileImageResponse && message.Next == true)
                {
                    string p = Path.GetTempPath() + "\\LANShare";
                    p = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "tmp\\";
                    Directory.CreateDirectory(p);
                    FileStream f = new FileStream(p + from.SessionId + ".jpg", FileMode.OpenOrCreate, FileAccess.Write);
                    new FileDownloadHelper().ReceiveFile(f, client);
                    f.Close();
                    from.ProfilePicture = new BitmapImage(new Uri(p + from.SessionId + ".jpg", UriKind.Absolute));
                }
            }catch(SocketException ex)
            {
            }
            catch (IOException ex)
            {
            }
        }

        private void HandleClient(TcpClient client, CancellationToken ct)
        {
            ConnectionMessage message = ReadMessage(client);
            if (message == null)
                return;
            switch (message.MessageType)
            {
                case MessageType.IpcBaseFolder:
                    List<string> files = new List<string>();
                    do
                    {
                        files.Add(message.Message as string);
                        message = ReadMessage(client);
                    } while (message.Next);
                    files.Add(message.Message as string);
                    OnSendRequested(files);
                    break;
                case MessageType.ProfileImageRequest:
                    try
                    {
                        FileStream f = File.OpenRead(Configuration.UserPicPath);
                        ConnectionMessage response =
                            new ConnectionMessage(MessageType.ProfileImageResponse, true, null);
                        SendMessage(client, response);
                        new FileUploadHelper().SendFile(f,client);
                        f.Close();
                    }
                    catch (Exception ex)
                    {
                        if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
                        {
                            ConnectionMessage response =
                                new ConnectionMessage(MessageType.ProfileImageResponse, false, null);
                            SendMessage(client, response);
                        }
                    }
                    break;
                case MessageType.FileUploadRequest:
                    User from = JsonConvert.DeserializeObject<User>(message.Message.ToString());
                    string username = from.NickName != null ? from.NickName : from.Name;

                    //TODO Ask user for permission
                    if (Configuration.FileAcceptanceMode.Equals(EFileAcceptanceMode.AskAlways))
                    {
                        bool rejected = false;
                        OnTransferRequested(from);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            lock (l)
                            {
                                ConfirmationWindow C = new ConfirmationWindow("Incoming transfer from " + username + ".\nDo you want to accept it ?");
                                C.ShowDialog();
                                if (C.DialogResult == false)
                                {
                                    message = new ConnectionMessage(MessageType.FileUploadResponse, false, null);
                                    SendMessage(client, message);
                                    rejected = true;
                                }
                            }
                        });

                        requestSeen?.Invoke(this, null);

                        if (rejected) {

                            break;
                        }
                            
                        


                    }
                    //TODO Ask user for permission
                    message = new ConnectionMessage(MessageType.FileUploadResponse, true, null);

                    SendMessage(client, message);
                    message = ReadMessage(client);
                    if (message.MessageType != MessageType.TotalUploadSize)
                        break;

                    //Ask for path to save files
                    string savePath = null;
                    if (Configuration.FileSavePathMode.Equals(EFileSavePathMode.AskForPath))
                    {
                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                        folderBrowserDialog.Description = "Save transmission from " + username;
                        folderBrowserDialog.RootFolder = Environment.SpecialFolder.Personal;
                        DialogResult dialogResult = folderBrowserDialog.ShowDialog();
                        if(dialogResult != DialogResult.OK)
                        {
                            break;
                        }
                        savePath = folderBrowserDialog.SelectedPath;
                    }else if (Configuration.FileSavePathMode.Equals(EFileSavePathMode.UseCustom))
                    {
                        savePath = Configuration.CustomSavePath;
                    }
                    else
                    {
                        savePath = Configuration.DefaultSavePath;
                    }
                    
                    
                    FileDownloadHelper helper = new FileDownloadHelper();
                    
                        helper.Counterpart = from;
                        helper.Status = TransferCompletitionStatus.Receiving;
                        OnUploadAccepted(helper);
                    
                    helper.HandleFileDownload(client, savePath, (long)message.Message);
                    break;
            }
            client.Close();
        }

        
        public bool SendFileList(string[] files)
        {
            try
            {
                TcpClient client = new TcpClient(IPAddress.Loopback.ToString(), Configuration.TcpPort);
                ConnectionMessage message = new ConnectionMessage(MessageType.IpcBaseFolder, true, files[0]);
                //SendMessage(client.Client,message);
                SendMessage(client, message);
                message.MessageType = MessageType.IpcElement;
                for (int i = 1; i < files.Length - 1; i++)
                {
                    message.Message = files[i];
                    SendMessage(client, message);
                }
                message.Message = files[files.Length - 1];
                message.Next = false;
                SendMessage(client, message);

                client.Close();
                return true;
            }
            catch (SocketException)
            {
                //Most likely failed to connect
                return false;
            }
        }
        internal static ConnectionMessage ReadMessage(TcpClient from)
        {
            NetworkStream ns = from.GetStream();
            byte[] messageSize = new byte[sizeof(Int32)];
            int bytesRed = ns.Read(messageSize, 0, messageSize.Length);
            if (bytesRed <= 0)
            {
                //NotificationWindow C = new NotificationWindow("Connection aborted.");
                //C.ShowDialog();
                
                throw new OperationCanceledException();
            }
            int messageLength = BitConverter.ToInt32(messageSize, 0);
            int toRead = IPAddress.NetworkToHostOrder(messageLength);
            byte[] readVector = new byte[toRead];
            int red = ns.Read(readVector, 0, readVector.Length);
            while (red < toRead)
            {
                red += ns.Read(readVector, red,toRead-red);
            }
            try
            {
                return bytesRed <= 0 ? null : ConnectionMessage.Deserialize(readVector);
            }catch(Exception ex)
            {
                return null;
            }
        }

        internal static void SendMessage(TcpClient to, ConnectionMessage message)
        {
            NetworkStream ns = to.GetStream();
            byte[] data = ConnectionMessage.Serialize(message);
            byte[] dataSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
            ns.Write(dataSize, 0, dataSize.Length);
            ns.Flush();
            ns.Write(data, 0, data.Length);
            ns.Flush();
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
        

        private void OnSendRequested(List<string> l)
        {
            FileSendRequested?.Invoke(this, l);
        }

        private void OnTransferRequested(User u)
        {
            TransferRequested?.Invoke(this, u);
        }


        protected virtual void OnUploadAccepted(IFileTransferHelper t)
        {
            UploadAccepted?.Invoke(this, t);
        }
        
    }
}
