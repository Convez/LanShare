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
using System.ComponentModel;

namespace LANshare.Model
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        private string _name;
        private string _nickname;
        [NonSerialized] private ImageSource _profilepicture;
        private EUserAdvertisementMode _privacymode;
        [field: NonSerializedAttribute()] public event PropertyChangedEventHandler PropertyChanged;


        public string Name
        {
            get => _name;
            set
            {
                _name = Environment.ExpandEnvironmentVariables(value);
                OnPropertyChanged("Name");
            }
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
                    Properties.Settings.Default.Save();
                }
                OnPropertyChanged("NickName");
            }
        }

        public String PrivacyMode
        {
            get
            {
                //if(this== Configuration.CurrentUser)
                //{
                //    return LANshare.Properties.Settings.Default.UserAdvertisementMode.ToString(); //this is realtime check
                //}
                return _privacymode.ToString(); //this is for checking another's user privacymode
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
                OnPropertyChanged("ProfilePicture");
            }
        }
        //Session Id
        public object SessionId { get=>_sessionId; set=>Interlocked.Exchange(ref _sessionId,value); }

        private object _sessionId;
        //User ip address
        [NonSerialized] public System.Net.IPAddress userAddress;
        
        // Tcp port listening for file upload requests for user
        public int TcpPortTo { get; set; }
        public User(string name, int tcpPortTo , EUserAdvertisementMode privacymode, Uri profilePicUri=null, string nickName=null)
        {
            Name = name;
            TcpPortTo = tcpPortTo;
            NickName = nickName;
            _privacymode = privacymode;
            if(profilePicUri==null)
            {
                if(this==Model.Configuration.CurrentUser)
                {
                    try
                    {
                        _profilepicture = new BitmapImage(new Uri("Media/Images/UserImages/customPic.jpg", UriKind.Relative));

                    }
                    catch (Exception e) when (e is ArgumentException || e is ArgumentNullException || e is FileNotFoundException)
                    {
                        try
                        {
                            _profilepicture = new BitmapImage(new Uri("Media/Images/UserImages/defaultPic.jpg", UriKind.Relative));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            _profilepicture = null;
                        }
                    }
                }
                else
                {
                    try
                    {
                        _profilepicture = new BitmapImage(new Uri("Media/Images/UserImages/defaultPic.jpg", UriKind.Relative));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        _profilepicture = null;
                    }
                }
            }else
            {
                try
                {
                    _profilepicture = new BitmapImage(profilePicUri);
                }
                catch (Exception e) when (e is ArgumentException || e is ArgumentNullException || e is FileNotFoundException)
                {
                    try
                    {
                        _profilepicture = new BitmapImage(new Uri("Media/Images/UserImages/defaultPic.jpg", UriKind.Relative));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        _profilepicture = null;
                    }
                }
            }

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
        public void SetPrivacyMode()
        {
            if (this == Model.Configuration.CurrentUser)
            {
                if (Properties.Settings.Default.UserAdvertisementMode == EUserAdvertisementMode.Private) _privacymode = EUserAdvertisementMode.Public;       //allows to set privacy only for local user
                else _privacymode = EUserAdvertisementMode.Private;
                Properties.Settings.Default.UserAdvertisementMode = _privacymode;
                Properties.Settings.Default.Save();
                OnPropertyChanged("PrivacyMode");
            }
        }

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
    }
}
