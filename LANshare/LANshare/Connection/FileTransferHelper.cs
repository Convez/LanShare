using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LANshare.Model;
using Newtonsoft.Json;

namespace LANshare.Connection
{
    public enum TransferCompletitionStatus
    {
        Sending,
        Completed,
        Canceled,
        Error,
        Requested,
        Receiving,
        Refused
    }

    public interface IFileTransferHelper : INotifyPropertyChanged
    {
        event EventHandler<TransferCompletitionStatus> TransferCompleted;
        event EventHandler<TcpClient> cancelRequested;

        User Counterpart { get; set; }

        TransferCompletitionStatus Status { get; set; }


        string FileName { get; set; }
        string destPath { get; set; }

        FileTransferProgressChangedArgs Args { get; set; }

        void Cancel();
    }

    public class FileDownloadHelper : IFileTransferHelper
    {
        public event EventHandler<TcpClient> cancelRequested;
        public event EventHandler<TransferCompletitionStatus> TransferCompleted;
        public event PropertyChangedEventHandler PropertyChanged;


        private TcpClient client;
        private List<string> filesDownloaded = new List<string>();
        private List<string> foldersDownloaded = new List<string>();
        private User _counterpart;
        private TransferCompletitionStatus _status;

        private string _filename;

        private FileTransferProgressChangedArgs _args;
        public FileTransferProgressChangedArgs Args
        {
            get => _args;
            set
            {
                _args = value;
                OnPropertyChanged("Args");
            }

        }
        public string destPath { get; set; }


        public string FileName { get => _filename;
            set
            {
                _filename = value;
                OnPropertyChanged("FileName");
            }
        }
        public User Counterpart { get => _counterpart;
            set {
                _counterpart = value;
                OnPropertyChanged("Counterpart");
            } }
        public TransferCompletitionStatus Status { get => _status;
            set {
                _status = value;
                OnPropertyChanged("Status");
            } }


        public FileDownloadHelper() { }
        public FileDownloadHelper(TcpClient c) {
            client = c;
        }

        public void HandleFileDownload(string destinationPath, long totalSize) => HandleFileDownload(client, destinationPath, totalSize);
        public void HandleFileDownload(TcpClient from,string destinationPath, long totalSize)
        {
            destPath = destinationPath;

            if (client == null) client = from;
            Stopwatch stopwatch = null;
            try
            {
                stopwatch = new Stopwatch();
                stopwatch.Reset();
                stopwatch.Start();
                ReceiveFiles(from, destinationPath, totalSize, 0,stopwatch);
                TransferCompleted?.Invoke(this, TransferCompletitionStatus.Completed);
                Status = TransferCompletitionStatus.Completed;
                Args = new FileTransferProgressChangedArgs(totalSize, totalSize, 100, TimeSpan.FromMilliseconds(0));
                TransferCompleted?.Invoke(this, TransferCompletitionStatus.Completed);
                stopwatch.Stop();
            }
            catch (Exception ex)
            {
                stopwatch?.Stop();
                //If the download goes bad -> delete
                if (ex is ObjectDisposedException || ex is SocketException || ex is OperationCanceledException)
                {
                    filesDownloaded.ForEach(File.Delete);
                    foldersDownloaded.Reverse();
                    foldersDownloaded.Where(x => !Directory.EnumerateFileSystemEntries(x).Any()).ToList().ForEach(Directory.Delete);
                    Status = TransferCompletitionStatus.Error;
                    TransferCompleted?.Invoke(this, TransferCompletitionStatus.Error);

                    if (ex is OperationCanceledException) {
                        Args = new FileTransferProgressChangedArgs(totalSize, totalSize, Args.DownloadPercentage, TimeSpan.FromMilliseconds(0));
                        Status = TransferCompletitionStatus.Canceled;
                        OnCanceled();
                    }
                }
            }
        }
        private long ReceiveFiles(TcpClient client, string basePath, long totSize, long currSize,Stopwatch stopwatch)
        {
            ConnectionMessage message = TCP_Comunication.ReadMessage(client);
            while (message.MessageType != MessageType.EndDirectory)
            {
                switch (message.MessageType)
                {
                    case MessageType.NewFile:
                        FileStream f = null;
                        try
                        {
                            string path = Path.Combine(basePath, message.Message as string);
                            if (File.Exists(path))
                            {
                                string fileNoExt = Path.GetFileNameWithoutExtension(path);
                                string ext = Path.GetExtension(path);
                                int fileCount = -1;
                                do { fileCount++; } while (File.Exists(Path.Combine(basePath, fileNoExt + "(" + fileCount.ToString() + ")" + ext)));
                                path =Path.Combine(basePath, fileNoExt + "(" + fileCount.ToString() + ")" + ext);
                            }
                            FileName = Path.GetFileName(path);
                            f = File.Create(path);
                            filesDownloaded.Add(path);
                            ReceiveFile(f, client, stopwatch,totSize, currSize);
                            f.Close();
                            currSize += new FileInfo(path).Length;
                        }catch(Exception e)
                        {
                            f?.Close();
                            throw e;
                        }
                        break;
                    case MessageType.NewDirectory:
                        string p = Path.Combine(basePath, message.Message as string);
                        Directory.CreateDirectory(p);
                        foldersDownloaded.Add(p);
                        currSize = ReceiveFiles(client, p, totSize, currSize, stopwatch);
                        break;
                    case MessageType.OperationCanceled:
                        throw new OperationCanceledException();
                }
                message = TCP_Comunication.ReadMessage(client);
            }
            return currSize;
        }

        internal void ReceiveFile(FileStream to, TcpClient from, Stopwatch stopWatch,long totSize =1, long curSize = 1)
        {
            ConnectionMessage message = TCP_Comunication.ReadMessage(from);
            byte[] data;

            long remainingTime = 0;
            long localCurr=0;
            List<long> previousTimes = new List<long>();
            while (message.Next)
            {
                if (message.MessageType == MessageType.OperationCanceled)
                    throw new OperationCanceledException();
                data = Convert.FromBase64String(message.Message as string);
                to.Write(data, 0, data.Length);
                long newCurr = curSize + data.Length;
                localCurr += data.Length;
                float percentage = ((float)newCurr / (float)totSize)*100;

                curSize = newCurr;
                if (stopWatch.ElapsedMilliseconds > 500)
                {
                    remainingTime = ( totSize - newCurr ) / localCurr * stopWatch.ElapsedMilliseconds;

                    previousTimes.Add(remainingTime);
                    if (previousTimes.Count > 10)
                    {
                        previousTimes = previousTimes.GetRange(previousTimes.Count - 10, 10);
                    }
                    long sum = previousTimes.Sum();
                    remainingTime = sum / previousTimes.Count;
                    stopWatch.Restart();
                    localCurr = 0;
                }
                OnProgressChanged(
                    new FileTransferProgressChangedArgs(totSize, curSize, (int)percentage, TimeSpan.FromMilliseconds(remainingTime)));
                message = TCP_Comunication.ReadMessage(from);
            }
        }

        protected virtual void OnProgressChanged(FileTransferProgressChangedArgs e)
        {
            Args = e;
        }

        public void Cancel()
        {
            try
            {
                ConnectionMessage message = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                TCP_Comunication.SendMessage(client, message);
                OnCanceled();
            }
            catch (Exception e) { }
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }

        private void OnCanceled()
        {
            cancelRequested?.Invoke(this, client);
            Status = TransferCompletitionStatus.Canceled;
        }
    }

    public class FileUploadHelper : IFileTransferHelper
    {
        public event EventHandler<TcpClient> cancelRequested;
        public event EventHandler<TransferCompletitionStatus> TransferCompleted;
        public event PropertyChangedEventHandler PropertyChanged;

        private FileTransferProgressChangedArgs _args;
        public FileTransferProgressChangedArgs Args
        {
            get => _args;
            set
            {
                _args = value;
                OnPropertyChanged("Args");
            }

        }
        public string destPath { get; set; }

        private TcpClient client;
        private CancellationTokenSource cts;
        private CancellationToken ctok;
        private User _counterpart;
        private TransferCompletitionStatus _status;
        private string _filename;
        public string FileName
        {
            get => _filename;
            set
            {
                _filename = value;
                OnPropertyChanged("FileName");
            }
        }

        public User Counterpart
        {
            get => _counterpart;
            set
            {
                _counterpart = value;
                OnPropertyChanged("Counterpart");
            }
        }
        public TransferCompletitionStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public bool InitFileSend(User to, List<string> files, CancellationToken ct, string subject = null)
        {
            cts = new CancellationTokenSource();
            destPath = files.ElementAt(0);
            ctok = cts.Token;
            client = new TcpClient(to.UserAddress.ToString(), to.TcpPortTo);
            ConnectionMessage message = new ConnectionMessage(MessageType.FileUploadRequest, true, Configuration.CurrentUser);
            TCP_Comunication.SendMessage(client, message);
            message = TCP_Comunication.ReadMessage(client);
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        ConnectionMessage isCancelRequested= TCP_Comunication.ReadMessage(client);
                        if (isCancelRequested.MessageType == MessageType.OperationCanceled)
                        {
                            Status = TransferCompletitionStatus.Canceled;
                            cts.Cancel();
                        }
                    }
                }catch(Exception ex)
                {

                }
            });
            if (message.MessageType == MessageType.FileUploadResponse)
            {
                if (message.Next == false)
                {
                    Status = TransferCompletitionStatus.Refused;
                    cancelRequested?.Invoke(this, client);
                    return false;
                }

                Status = TransferCompletitionStatus.Sending;

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

                Stopwatch stopwatch = null;
                try
                {
                    if (ctok.IsCancellationRequested)
                    {
                        ConnectionMessage me = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                        TCP_Comunication.SendMessage(client, me);
                        throw new OperationCanceledException();
                    }
                    message = new ConnectionMessage(MessageType.TotalUploadSize, true, totalSize);
                    TCP_Comunication.SendMessage(client, message);
                    stopwatch = new Stopwatch();
                    stopwatch.Reset();
                    stopwatch.Start();
                    SendFiles(client, folder, files, ctok, stopwatch,totalSize, 0);
                    TransferCompleted?.Invoke(this, TransferCompletitionStatus.Completed);
                    Status = TransferCompletitionStatus.Completed;
                    Args = new FileTransferProgressChangedArgs(totalSize, totalSize, 100, TimeSpan.FromMilliseconds(0));
                }
                catch (OperationCanceledException ex)
                {
                    client.Close();
                    Args = new FileTransferProgressChangedArgs(totalSize, totalSize, Args!=null?Args.DownloadPercentage:0, TimeSpan.FromMilliseconds(0));
                    OnCanceled();
                    stopwatch?.Stop();
                    return false;
                }catch (Exception ex)
                {
                    client.Close();
                    Args = new FileTransferProgressChangedArgs(totalSize, totalSize, Args != null ? Args.DownloadPercentage : 0, TimeSpan.FromMilliseconds(0));
                    Status = TransferCompletitionStatus.Error;
                    TransferCompleted?.Invoke(this, TransferCompletitionStatus.Error);
                    stopwatch?.Stop();
                    return false;
                }
                stopwatch?.Stop();
                client.Close();
                return true;
            }
            client.Close();
            return false;
        }

        private long SendFiles(TcpClient client, string baseFolder, List<string> files, CancellationToken ct, Stopwatch stopwatch,long totalSize=1, long currSize=1)
        {
            foreach (string file in files)
            {
                FileName = file;
                string path = Path.Combine(baseFolder, file);
                FileAttributes attr = File.GetAttributes(path);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    if (ctok.IsCancellationRequested)
                    {
                        ConnectionMessage me = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                        TCP_Comunication.SendMessage(client, me);
                        throw new OperationCanceledException();
                    }
                    ConnectionMessage message = new ConnectionMessage(MessageType.NewDirectory, true, file);
                    TCP_Comunication.SendMessage(client, message);
                    List<string> toSend = Directory.GetFileSystemEntries(path).ToList().Select(x => Path.GetFileName(x)).ToList();
                    
                    //toSend.AddRange(Directory.GetDirectories(path).Select(x => Path.GetDirectoryName(x)).ToList());
                    currSize = SendFiles(client, path, toSend, ct, stopwatch,totalSize,currSize);
                }
                else
                {
                    if (ctok.IsCancellationRequested)
                    {
                        ConnectionMessage me = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                        TCP_Comunication.SendMessage(client, me);
                        throw new OperationCanceledException();
                    }
                    ConnectionMessage message = new ConnectionMessage(MessageType.NewFile, true, file);
                    TCP_Comunication.SendMessage(client, message);
                    FileStream f = File.OpenRead(path);
                    SendFile(f, client, stopwatch,totalSize,currSize);
                    currSize += new FileInfo(path).Length;
                    f.Close();
                }
            }
            if (ctok.IsCancellationRequested)
            {
                ConnectionMessage me = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                TCP_Comunication.SendMessage(client, me);
                throw new OperationCanceledException();
            }
            ConnectionMessage mess = new ConnectionMessage(MessageType.EndDirectory, false, null);
            TCP_Comunication.SendMessage(client, mess);
            return currSize;
        }
        

        internal void SendFile(FileStream from, TcpClient to, Stopwatch stopWatch, long totSize = 1, long curSize = 1)
        {
            byte[] block = new byte[4096];
            int bytesRed = from.Read(block, 0, block.Length);
            
            long remainingTime = 0;
            long localCurr = 0;
            List<long> previousTimes = new List<long>();
            do
            {
                if (ctok.IsCancellationRequested)
                {
                    ConnectionMessage me = new ConnectionMessage(MessageType.OperationCanceled, false, null);
                    TCP_Comunication.SendMessage(to, me);
                    throw new OperationCanceledException();
                }
                ConnectionMessage message = new ConnectionMessage(MessageType.FileChunk, true, Convert.ToBase64String(block));
                TCP_Comunication.SendMessage(to, message);
                long newCurr = curSize + bytesRed;
                localCurr += bytesRed;
                curSize = newCurr;
                float percentage = ((float)newCurr / (float)totSize) * 100;

                if (stopWatch.ElapsedMilliseconds > 500)
                {
                    remainingTime = (totSize - newCurr) / localCurr * stopWatch.ElapsedMilliseconds;
                    previousTimes.Add(remainingTime);
                    if (previousTimes.Count > 10)
                    {
                        previousTimes = previousTimes.GetRange(previousTimes.Count - 10, 10);
                    }
                    long sum = previousTimes.Sum();
                    remainingTime = sum / previousTimes.Count;
                    stopWatch.Restart();
                    localCurr = 0;
                }
                OnProgressChanged(
                    new FileTransferProgressChangedArgs(totSize, curSize, (int)percentage, TimeSpan.FromMilliseconds(remainingTime)));
                bytesRed = from.Read(block, 0, block.Length);
            } while (bytesRed > 0);
            ConnectionMessage mess = new ConnectionMessage(MessageType.FileChunk, false, null);
            TCP_Comunication.SendMessage(to, mess);
        }

        protected virtual void OnProgressChanged(FileTransferProgressChangedArgs e)
        {
            Args = e;
        }

        public void Cancel()
        {
            try
            {
                cts.Cancel();
                OnCanceled();

            }
            catch(Exception e) { }
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
        private void OnCanceled()
        {
            cancelRequested?.Invoke(this, client);
            Status = TransferCompletitionStatus.Canceled;
        }
    }

    public class FileTransferProgressChangedArgs : EventArgs
    {
        public long TotalTransferSize{ get; private set; }
        public long CurrentTransferedSize{ get; private set; }
        public int DownloadPercentage { get; set; }
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
