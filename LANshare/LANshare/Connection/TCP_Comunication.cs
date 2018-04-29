using LANshare.Model;
using System;
using System.Collections.Generic;
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

namespace LANshare.Connection
{
    
    class TCP_Comunication
    {
        private CancellationTokenSource shuttingDown;

        private Task serverTask;

        private List<TcpListener> listeners;

        public event EventHandler<List<string>> FileSendRequested;

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
            listeners?.ForEach((l)=>l.Stop());
            serverTask?.Wait();
        }

        public void RequestImage(User from)
        {
            TcpClient client = new TcpClient(from.UserAddress.ToString(), from.TcpPortTo);
            ConnectionMessage message = new ConnectionMessage(MessageType.ProfileImageRequest, false, null);
            SendMessage(client, message);
            message = ReadMessage(client);
            if (message.MessageType == MessageType.ProfileImageResponse && message.Next == true)
            {
                string p = Path.GetTempPath() + "\\LANShare";
                p = AppDomain.CurrentDomain.SetupInformation.ApplicationBase+"tmp\\";
                Directory.CreateDirectory(p);
                FileStream f = new FileStream(p+from.SessionId+".jpg", FileMode.OpenOrCreate, FileAccess.Write);
                new FileDownloadHelper().ReceiveFile(f, client);
                f.Close();
                from.ProfilePicture = new BitmapImage(new Uri(p+from.SessionId+".jpg", UriKind.Absolute));
            }
        }

        private void HandleClient(TcpClient client, CancellationToken ct)
        {
            ConnectionMessage message = ReadMessage(client);
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
                        FileStream f = File.OpenRead("Media/profile.jpg");
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
                    User from = message.Message as User;
                    string username = from.NickName != null ? from.NickName : from.Name;
                    //Ask user for permission
                    if (Configuration.FileAcceptanceMode.Equals(EFileAcceptanceMode.AskAlways))
                    {

                        DialogResult result = MessageBox.Show("Incoming transfer from " + username + ".\nDo you want to accept it?"
                            , "Incoming transfer requested", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                        {
                            message = new ConnectionMessage(MessageType.FileUploadResponse, false, null);
                            SendMessage(client, message);
                            break;
                        }
                    }

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
            byte[] messageSize = new byte[sizeof(long)];
            int bytesRed = ns.Read(messageSize, 0, messageSize.Length);
            if (bytesRed <= 0)
            {
                MessageBox.Show("Connection aborted");
                return null;
            }
            long messageLength = BitConverter.ToInt64(messageSize, 0);
            byte[] readVector = new byte[IPAddress.NetworkToHostOrder(messageLength)];
            bytesRed = ns.Read(readVector, 0, readVector.Length);
            return bytesRed <= 0 ? null : ConnectionMessage.Deserialize(readVector);
        }

        internal static void SendMessage(TcpClient to, ConnectionMessage message)
        {
            NetworkStream ns = to.GetStream();
            byte[] data = ConnectionMessage.Serialize(message);
            byte[] dataSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.LongLength));
            ns.Write(dataSize, 0, dataSize.Length);
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

        protected virtual void OnUploadAccepted(IFileTransferHelper t)
        {
            UploadAccepted?.Invoke(this, t);
        }
        
    }
}
