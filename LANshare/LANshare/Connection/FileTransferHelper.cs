using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LANshare.Model;

namespace LANshare.Connection
{
    

    public interface IFileTransferHelper
    {
        event EventHandler<FileTransferProgressChangedArgs> ProgressChanged;
        event EventHandler<TcpClient> cancelRequested;
        void Cancel();
    }

    public class FileDownloadHelper : IFileTransferHelper
    {
        public event EventHandler<FileTransferProgressChangedArgs> ProgressChanged;
        public event EventHandler<TcpClient> cancelRequested;
        private TcpClient client;
        private List<string> filesDownloaded = new List<string>();
        private List<string> foldersDownloaded = new List<string>();
        public FileDownloadHelper() { }
        public FileDownloadHelper(TcpClient c) {
            client = c;
        }

        public void HandleFileDownload(string destinationPath, long totalSize) => HandleFileDownload(client, destinationPath, totalSize);
        public void HandleFileDownload(TcpClient from,string destinationPath, long totalSize)
        {
            if (client == null) client = from;
            try
            {
                ReceiveFiles(from, destinationPath, totalSize, 0);
            }
            catch (Exception ex)
            {
                //If the download goes bad -> delete
                if (ex is ObjectDisposedException || ex is SocketException || ex is OperationCanceledException)
                {
                    filesDownloaded.ForEach(File.Delete);
                    foldersDownloaded.Reverse();
                    foldersDownloaded.Where(x => Directory.EnumerateFileSystemEntries(x).Any()).ToList().ForEach(Directory.Delete);
                }
            }
        }
        private void ReceiveFiles(TcpClient client, string basePath, long totSize, long currSize)
        {
            ConnectionMessage message = TCP_Comunication.ReadMessage(client);
            while (message.MessageType != MessageType.EndDirectory)
            {
                switch (message.MessageType)
                {
                    case MessageType.NewFile:
                        string path = Path.Combine(basePath, message.Message as string);
                        if (File.Exists(path)) {
                            int fileCount = -1;
                            do { fileCount++; } while (File.Exists(path + "(" + fileCount.ToString() + ")"));
                            path = path + "(" + fileCount.ToString() + ")";
                        }

                        FileStream f = File.Create(path);
                        filesDownloaded.Add(path);
                        ReceiveFile(f, client, totSize, currSize, Environment.TickCount);
                        currSize += new FileInfo(path).Length;
                        f.Close();
                        break;
                    case MessageType.NewDirectory:
                        string p = Path.Combine(basePath, message.Message as string);
                        Directory.CreateDirectory(p);
                        foldersDownloaded.Add(p);
                        ReceiveFiles(client, p, totSize, currSize);
                        break;
                    case MessageType.OperationCanceled:
                        throw new OperationCanceledException();
                }
                message = TCP_Comunication.ReadMessage(client);
            }
        }

        internal void ReceiveFile(FileStream to, TcpClient from, long totSize =1, long curSize = 1, long previousInstant=1)
        {
            ConnectionMessage message = TCP_Comunication.ReadMessage(from);
            byte[] data;
            while (message.Next)
            {
                if (message.MessageType == MessageType.OperationCanceled)
                    throw new OperationCanceledException();
                data = message.Message as byte[];
                to.Write(data, 0, data.Length);
                long newCurr = curSize + data.Length;
                int percentage = (int)(newCurr / totSize);
                long remainingTime = totSize / (newCurr - curSize) * (Environment.TickCount - previousInstant);
                curSize = newCurr;
                previousInstant = Environment.TickCount;
                OnProgressChanged(
                    new FileTransferProgressChangedArgs(totSize, curSize, percentage, TimeSpan.FromTicks(remainingTime)));
                message = TCP_Comunication.ReadMessage(from);
            }
        }

        protected virtual void OnProgressChanged(FileTransferProgressChangedArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public void Cancel()
        {
            try
            {
                ConnectionMessage message = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                TCP_Comunication.SendMessage(client, message);
                client.Close();
            }
            catch (Exception e) { }
        }
    }

    public class FileUploadHelper : IFileTransferHelper
    {
        public event EventHandler<FileTransferProgressChangedArgs> ProgressChanged;
        public event EventHandler<TcpClient> cancelRequested;
        private TcpClient client;
        private CancellationTokenSource cts;
        private CancellationToken ctok;

        public bool InitFileSend(User to, List<string> files, CancellationToken ct, string subject = null)
        {
            cts = new CancellationTokenSource();
            ctok = cts.Token;
            client = new TcpClient(to.UserAddress.ToString(), to.TcpPortTo);
            Task.Run(() =>
            {
                var msg = TCP_Comunication.ReadMessage(client);
                if (msg.MessageType == MessageType.OperationCanceled)
                {
                    cancelRequested?.Invoke(this, client);
                }
            });
            ConnectionMessage message = new ConnectionMessage(MessageType.FileUploadRequest, true, Configuration.CurrentUser);
            TCP_Comunication.SendMessage(client, message);
            message = TCP_Comunication.ReadMessage(client);
            if (message.MessageType == MessageType.FileUploadResponse)
            {
                if (message.Next == false)
                    return false;
                string folder = files.First();
                files.Remove(folder);

                //calculate total size
                long totalSize = files.Sum((s) =>
                {
                    string path = Path.Combine(folder, s);
                    FileAttributes attr = File.GetAttributes(path);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(x => x.Length);
                    }
                    return new FileInfo(path).Length;
                });


                message = new ConnectionMessage(MessageType.TotalUploadSize, true, totalSize);
                TCP_Comunication.SendMessage(client, message);
                try
                {
                    SendFiles(client, folder, files, ctok, totalSize, 0);
                }
                catch (OperationCanceledException ex)
                {
                    client.Close();
                    return false;
                }
                client.Close();
                return true;
            }
            client.Close();
            return false;
        }

        private void SendFiles(TcpClient client, string baseFolder, List<string> files, CancellationToken ct, long totalSize=1, long currSize=1)
        {
            foreach (string file in files)
            {
                string path = Path.Combine(baseFolder, file);
                FileAttributes attr = File.GetAttributes(path);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    ConnectionMessage message = new ConnectionMessage(MessageType.NewDirectory, true, file);
                    TCP_Comunication.SendMessage(client, message);
                    SendFiles(client, path, Directory.GetFiles(path).Select(x => Path.GetFileName(x)).ToList(), ct,totalSize,currSize);
                }
                else
                {
                    ConnectionMessage message = new ConnectionMessage(MessageType.NewFile, true, file);
                    TCP_Comunication.SendMessage(client, message);
                    FileStream f = File.OpenRead(path);
                    SendFile(f, client,totalSize,currSize,Environment.TickCount);
                    f.Close();
                }
            }
            ConnectionMessage mess = new ConnectionMessage(MessageType.EndDirectory, false, null);
            TCP_Comunication.SendMessage(client, mess);
        }
        

        internal void SendFile(FileStream from, TcpClient to, long totSize = 1, long curSize = 1, long previousInstant = 1)
        {
            byte[] block = new byte[2048];
            int bytesRed = from.Read(block, 0, block.Length);
            do
            {
                if (ctok.IsCancellationRequested)
                {
                    ConnectionMessage me = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                    TCP_Comunication.SendMessage(to, me);
                    throw new OperationCanceledException();
                }
                ConnectionMessage message = new ConnectionMessage(MessageType.FileChunk, true, block);
                TCP_Comunication.SendMessage(to, message);
                long newCurr = curSize + bytesRed;
                int percentage = (int)(newCurr / totSize);
                long remainingTime = totSize / (newCurr - curSize) * (Environment.TickCount - previousInstant);
                curSize = newCurr;
                previousInstant = Environment.TickCount;
                OnProgressChanged(
                    new FileTransferProgressChangedArgs(totSize, curSize, percentage, TimeSpan.FromTicks(remainingTime)));
                bytesRed = from.Read(block, 0, block.Length);
            } while (bytesRed > 0);
            ConnectionMessage mess = new ConnectionMessage(MessageType.FileChunk, false, null);
            TCP_Comunication.SendMessage(to, mess);
        }

        protected virtual void OnProgressChanged(FileTransferProgressChangedArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public void Cancel()
        {
            try
            {
                ConnectionMessage message = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                TCP_Comunication.SendMessage(client, message);
                client.Close();
            }
            catch(Exception e) { }
        }
    }

    public class FileTransferProgressChangedArgs : EventArgs
    {
        public long TotalTransferSize{ get; private set; }
        public long CurrentTransferedSize{ get; private set; }
        public int DownloadPercentage { get; private set; }
        public TimeSpan RemainingTime { get; private set; }

        internal FileTransferProgressChangedArgs(long totalTransferSize, long currentTransferedSize,
            int downloadPercentage, TimeSpan remainingTime)
        {
            TotalTransferSize = totalTransferSize;
            CurrentTransferedSize = currentTransferedSize;
            DownloadPercentage = downloadPercentage;
            RemainingTime = remainingTime;
        }
    }
}
