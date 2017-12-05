using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LANshare.Connection;

namespace LANshare.Model
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        public string Name
        {
            get => _name;
            set => _name = Environment.ExpandEnvironmentVariables(value);
        }

        [NonSerialized]
        private ImageSource _profile;
        public ImageSource ProfileImage
        {
            get => _profile;
            set
            {
                _profile = value;
                _profile.Freeze();
                OnPropertyChanged(this,"ProfileImage");
            }
        }

        private string _name;
        public string NickName { get; set; }

        //Session Id
        public object SessionId { get=>_sessionId; set=>Interlocked.Exchange(ref _sessionId,value); }

        private object _sessionId;
        //User ip address
        [NonSerialized] private System.Net.IPAddress _userAddress;
        
        public System.Net.IPAddress UserAddress
        {
            get => _userAddress;
            set => _userAddress = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        // Tcp port listening for file upload requests for user
        public int TcpPortTo { get; set; }
        public User(string name, int tcpPortTo , string nickName=null)
        {
            Name = name;
            TcpPortTo = tcpPortTo;
            NickName = nickName;
        }

        public void SetupImage()
        {
            ProfileImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase+"Media\\default_pic.jpg", UriKind.Absolute));
            TCP_Comunication com = new TCP_Comunication();
            Task.Run(() => com.RequestImage(this));
        }
        public override string ToString()
        {
            return Name + " (" + UserAddress.ToString() + ")";
        }

        private void OnPropertyChanged(object sender, string propertyName)
        {
            Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            if (dispatcher != null)
            {
                PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName)));
            }
        }
        public static string GenerateSessionId()
        {
            Random generator = new Random();
            string randNum = generator.Next(int.MinValue, int.MaxValue).ToString();
            HashAlgorithm hashAlg = SHA512.Create();
            byte[] hashed = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(randNum));
            return BitConverter.ToString(hashed).Replace("-","");
        }
    }
}
