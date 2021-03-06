﻿using System;
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
using System.Windows.Threading;
using LANshare.Connection;
using Newtonsoft.Json;
namespace LANshare.Model
{
    [Serializable]
    public class User : INotifyPropertyChanged
    {
        private string _name;
        private string _nickname;
        [NonSerialized] [JsonIgnore] private ImageSource _profilepicture;
        private EUserAdvertisementMode _privacymode;
        [field: NonSerializedAttribute()] public event PropertyChangedEventHandler PropertyChanged;

        [JsonProperty("Name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = Environment.ExpandEnvironmentVariables(value);
                OnPropertyChanged("Name");
            }
        }
        [JsonIgnore]
        public string IpAddress
        {
            get => UserAddress.ToString();
            
        }
        [JsonProperty("NickName")]
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
                NickModTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                OnPropertyChanged("NickName");
            }
        }
        private long _nickModTime;
        public long NickModTime
        {
            get;set;
        }
        [JsonProperty("PrivacyMode")]
        public String PrivacyMode
        {
            get
            {
                return _privacymode.ToString(); 
            }
        }

        [JsonIgnore]
        public ImageSource ProfilePicture
        {
            get => _profilepicture;
            set
            {
                _profilepicture = value;
                _profilepicture.Freeze();
                OnPropertyChanged("ProfilePicture");
            }
       }
        private long _lastPicModification;
        public long LastPicModification
        {
            get;set;
        }
       
        //Session Id
        [JsonProperty("SessionId")]
        public object SessionId { get=>_sessionId; set=>Interlocked.Exchange(ref _sessionId,value); }

        private object _sessionId;
        //User ip address
        [NonSerialized] [JsonIgnore] private System.Net.IPAddress _userAddress;
        
        [JsonIgnore]
        public System.Net.IPAddress UserAddress
        {
            get => _userAddress;
            set => _userAddress = value;
        }
        

        [JsonProperty("TcpPortTo")]
        // Tcp port listening for file upload requests for user
        public int TcpPortTo { get; set; }
        public User(string name, int tcpPortTo , EUserAdvertisementMode privacymode, Uri profilePicUri=null, string nickName=null)
        {
            Name = name;
            TcpPortTo = tcpPortTo;
            NickName = nickName;
            _privacymode = privacymode;

            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;

                if (profilePicUri == null)
                {
                    

                    if (File.Exists(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Properties.Settings.Default.CustomPic))
                        bi.UriSource= new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Properties.Settings.Default.CustomPic, UriKind.Absolute);
                    else
                        bi.UriSource= new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Configuration.DefaultPicPath, UriKind.Absolute);
                    bi.EndInit();
                    ProfilePicture = bi;
                }
                else
                {
                    try
                    {
                        System.IO.File.Copy(profilePicUri.AbsolutePath, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Properties.Settings.Default.CustomPic, true);
                       
                        bi.UriSource= new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Properties.Settings.Default.CustomPic, UriKind.Absolute);
                        bi.EndInit();
                        ProfilePicture = bi;

                    }
                    catch (Exception e) when (e is ArgumentException || e is ArgumentNullException || e is FileNotFoundException)
                    {
                        try
                        {
                            bi.UriSource= new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Configuration.DefaultPicPath, UriKind.Absolute);
                            bi.EndInit();
                            ProfilePicture = bi;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _profilepicture = null;
            }
            LastPicModification = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public void SetupImage()
        {
            ProfilePicture = new BitmapImage(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase+Configuration.DefaultPicPath, UriKind.Absolute));
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
