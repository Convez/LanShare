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
using System.Drawing;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using LANshare.Connection;
using System.Windows.Threading;

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
            get => UserAddress.ToString();
            
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
                return _privacymode.ToString(); 
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
                _profilepicture.Freeze();
                OnPropertyChanged("ProfilePicture");
            }
       }

        
       

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
                        _profilepicture = new BitmapImage(new Uri("Media/Images/UserImages/"+ Properties.Settings.Default.CustomPic , UriKind.Relative));

                    }
                    catch (Exception e) when (e is ArgumentException || e is ArgumentNullException || e is FileNotFoundException)
                    {
                        try
                        {
                            _profilepicture = new BitmapImage(new Uri(LANshare.Properties.Settings.Default.DefaultPic , UriKind.Relative));
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
                        _profilepicture = new BitmapImage(new Uri(LANshare.Properties.Settings.Default.DefaultPic, UriKind.Relative));
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
                        _profilepicture = new BitmapImage(new Uri(LANshare.Properties.Settings.Default.DefaultPic, UriKind.Relative));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        _profilepicture = null;
                    }
                }
            }

        }

        public void SetupImage()
        {
            ProfilePicture = new BitmapImage(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase+"Media\\default_pic.jpg", UriKind.Absolute));
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
