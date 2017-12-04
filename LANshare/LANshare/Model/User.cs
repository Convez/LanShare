using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace LANshare.Model
{
    [Serializable]
    public class User 
    {
        private string _name;
        private string _nickname;
        [NonSerialized] private ImageSource _profilepicture;
        private EUserAdvertisementMode _privacymode;


        public string Name
        {
            get => _name;
            set => _name = Environment.ExpandEnvironmentVariables(value);
        }
        public string IpAddress
        {
            get => userAddress.ToString();
            
        }
        
        public string NickName
        {
            get
            {
                if (_nickname == null) return " ";
                else return _nickname;
            }
            set
            {
                _nickname = value;
                if(this==Model.Configuration.CurrentUser)
                {
                    LANshare.Properties.Settings.Default.UserNickName = _nickname;
                }
            }
        }

        public String PrivacyMode
        {
            get { return _privacymode.ToString(); }
            set
            {
                if (_privacymode == EUserAdvertisementMode.Private) _privacymode = EUserAdvertisementMode.Public;
                else _privacymode = EUserAdvertisementMode.Private;
                if (this == Model.Configuration.CurrentUser) LANshare.Properties.Settings.Default.UserAdvertisementMode = _privacymode;
            }
        }

        public ImageSource ProfilePicture
        {
            get
            {
                
                return _profilepicture;
            }
            set
            {
                _profilepicture = value;
              
            }
        }
        //Session Id
        public object SessionId { get=>_sessionId; set=>Interlocked.Exchange(ref _sessionId,value); }

        private object _sessionId;
        //User ip address
        [NonSerialized] public System.Net.IPAddress userAddress;
        
        // Tcp port listening for file upload requests for user
        public int TcpPortTo { get; set; }
        public User(string name, int tcpPortTo , Uri profilePicUri, string nickName=null)
        {
            Name = name;
            TcpPortTo = tcpPortTo;
            NickName = nickName;
            _profilepicture = new BitmapImage(profilePicUri);
        }

        public override string ToString()
        {
            return Name + " (" + userAddress.ToString() + ")";
        }

        public static string GenerateSessionId()
        {
            Random generator = new Random();
            string randNum = generator.Next(int.MinValue, int.MaxValue).ToString();
            HashAlgorithm hashAlg = SHA512.Create();
            byte[] hashed = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(randNum));
            return Encoding.UTF8.GetString(hashed);
        }
    }
}
