using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        AskForPath,
        UseCustomDefault
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
        public static int TcpPort { get; set; }
        public static EFileAcceptanceMode FileAcceptanceMode { get; set; }
        public static EFileSavePathMode FileSavePathMode { get; set; }
        public static EUserAdvertisementMode UserAdvertisementMode { get; set; }
        public static EAdvertisedUserMode AdvertisedUserMode { get; set; }
        public static string DefaultSavePath { get; set; }
        public static User CurrentUser { get; private set; }


        public static void LoadConfiguration()
        {
            UdpPort = Properties.Settings.Default.UdpPort;
            MulticastAddress = System.Net.IPAddress.Parse(Properties.Settings.Default.MulticastAddress);
            TcpPort = Properties.Settings.Default.TcpPort;
            FileAcceptanceMode = Properties.Settings.Default.FileAcceptanceMode;
            FileSavePathMode = Properties.Settings.Default.FileSavePathMode;
            UserAdvertisementMode = Properties.Settings.Default.UserAdvertisementMode;
            AdvertisedUserMode = Properties.Settings.Default.AdvertisedUserMode;
            DefaultSavePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.DefaultSavePath));
            CurrentUser = new User(Properties.Settings.Default.DefaultUser, TcpPort ,Properties.Settings.Default.UserNickName);
        }

        public static void SaveConfiguration()
        {
            Properties.Settings.Default.TcpPort = TcpPort;
            Properties.Settings.Default.FileAcceptanceMode = FileAcceptanceMode;
            Properties.Settings.Default.FileSavePathMode = FileSavePathMode;
            Properties.Settings.Default.UserAdvertisementMode = UserAdvertisementMode;
            Properties.Settings.Default.AdvertisedUserMode = AdvertisedUserMode;
            Properties.Settings.Default.DefaultSavePath = DefaultSavePath;
            Properties.Settings.Default.DefaultUser = CurrentUser.Name;
            Properties.Settings.Default.UserNickName = CurrentUser.NickName;
            Properties.Settings.Default.Save();
        }

        public static void RestoreDefaultConfiguration()
        {
            Properties.Settings.Default.Reset();
        }
    }
}
