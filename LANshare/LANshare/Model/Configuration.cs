using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LANshare.Model
{
    [Serializable]
    public enum EFileAcceptanceMode
    {
        AcceptAll,
        AskAlways
    }
    [Serializable]
    public enum EFileSavePathMode
    {
        UseDefault,
        UseCustom,
        AskForPath
    }
    [Serializable]
    public enum EUserAdvertisementMode
    {
        Public,
        Private
    }
    [Serializable]
    public enum EAdvertisedUserMode
    {
        UseComputerUserLogin,
        CustomUser
    }
    
    public static class Configuration
    {
        public static int UdpPort { get; private set; }
        public static System.Net.IPAddress MulticastAddress{get; private set; }
        public static int UdpPacketsIntervalMilliseconds { get; private set; }
        public static int TCPConnectionTimeoutMilliseconds { get; private set; }
        public static int TcpPort { get; set; }
        public static EFileAcceptanceMode FileAcceptanceMode { get; set; }
        public static EFileSavePathMode FileSavePathMode { get; set; }
        public static EUserAdvertisementMode UserAdvertisementMode { get; set; }
        public static EAdvertisedUserMode AdvertisedUserMode { get; set; }
        public static string DefaultSavePath { get; private set; }
        public static string CustomSavePath { get; set; }
        public static User CurrentUser { get; private set; }
        public static int UserValidityMilliseconds { get; private set; }

        public static void LoadConfiguration()
        {
            UdpPort = Properties.Settings.Default.UdpPort;
            MulticastAddress = System.Net.IPAddress.Parse(Properties.Settings.Default.MulticastAddress);
            UdpPacketsIntervalMilliseconds = Properties.Settings.Default.UdpPacketsIntervalMilliseconds;
            TCPConnectionTimeoutMilliseconds = Properties.Settings.Default.TcpConnectionTimeoutMilliseconds;
            TcpPort = Properties.Settings.Default.TcpPort;
            FileAcceptanceMode = Properties.Settings.Default.FileAcceptanceMode;
            FileSavePathMode = Properties.Settings.Default.FileSavePathMode;
            UserAdvertisementMode = Properties.Settings.Default.UserAdvertisementMode;
            AdvertisedUserMode = Properties.Settings.Default.AdvertisedUserMode;
            DefaultSavePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.DefaultSavePath));
            CurrentUser = new User(Properties.Settings.Default.DefaultUser, TcpPort , Properties.Settings.Default.UserAdvertisementMode , null , Properties.Settings.Default.UserNickName);
            CurrentUser.userAddress = Dns.GetHostAddresses(Dns.GetHostName())
                .FirstOrDefault((ip) => ip.AddressFamily == AddressFamily.InterNetwork);
            UserValidityMilliseconds = Properties.Settings.Default.UserValidityMilliseconds;
            CustomSavePath = Properties.Settings.Default.CustomSavePath;
        }

        public static void SaveConfiguration()
        {
            Properties.Settings.Default.TcpPort = TcpPort;
            Properties.Settings.Default.FileAcceptanceMode = FileAcceptanceMode;
            Properties.Settings.Default.FileSavePathMode = FileSavePathMode;
            Properties.Settings.Default.UserAdvertisementMode = UserAdvertisementMode;
            Properties.Settings.Default.AdvertisedUserMode = AdvertisedUserMode;
            Properties.Settings.Default.CustomSavePath = CustomSavePath;
            Properties.Settings.Default.DefaultUser = CurrentUser.Name;
            Properties.Settings.Default.UserNickName = CurrentUser.NickName;
            Properties.Settings.Default.Save();
        }

        public static void RestoreDefaultConfiguration()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            LoadConfiguration();
        }
    }
}
