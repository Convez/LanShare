﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LANshare.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.6.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("44666")]
        public int UdpPort {
            get {
                return ((int)(this["UdpPort"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("42666")]
        public int TcpPort {
            get {
                return ((int)(this["TcpPort"]));
            }
            set {
                this["TcpPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AskAlways")]
        public global::LANshare.Model.EFileAcceptanceMode FileAcceptanceMode {
            get {
                return ((global::LANshare.Model.EFileAcceptanceMode)(this["FileAcceptanceMode"]));
            }
            set {
                this["FileAcceptanceMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("UseDefault")]
        public global::LANshare.Model.EFileSavePathMode FileSavePathMode {
            get {
                return ((global::LANshare.Model.EFileSavePathMode)(this["FileSavePathMode"]));
            }
            set {
                this["FileSavePathMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Public")]
        public global::LANshare.Model.EUserAdvertisementMode UserAdvertisementMode {
            get {
                return ((global::LANshare.Model.EUserAdvertisementMode)(this["UserAdvertisementMode"]));
            }
            set {
                this["UserAdvertisementMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("UseComputerUserLogin")]
        public global::LANshare.Model.EAdvertisedUserMode AdvertisedUserMode {
            get {
                return ((global::LANshare.Model.EAdvertisedUserMode)(this["AdvertisedUserMode"]));
            }
            set {
                this["AdvertisedUserMode"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%USERPROFILE%\\Documents\\LANshare")]
        public string DefaultSavePath {
            get {
                return ((string)(this["DefaultSavePath"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%USERNAME%")]
        public string DefaultUser {
            get {
                return ((string)(this["DefaultUser"]));
            }
            set {
                this["DefaultUser"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string UserNickName {
            get {
                return ((string)(this["UserNickName"]));
            }
            set {
                this["UserNickName"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("239.192.41.74")]
        public string MulticastAddress {
            get {
                return ((string)(this["MulticastAddress"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int UdpPacketsIntervalMilliseconds {
            get {
                return ((int)(this["UdpPacketsIntervalMilliseconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30000")]
        public int UserValidityMilliseconds {
            get {
                return ((int)(this["UserValidityMilliseconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12000")]
        public int TcpConnectionTimeoutMilliseconds {
            get {
                return ((int)(this["TcpConnectionTimeoutMilliseconds"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%USERPROFILE%\\Documents\\LANshare")]
        public string CustomSavePath {
            get {
                return ((string)(this["CustomSavePath"]));
            }
            set {
                this["CustomSavePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Media\\Images\\UserImages\\defaultPic.png")]
        public string DefaultPic {
            get {
                return ((string)(this["DefaultPic"]));
            }
            
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Media\\Images\\UserImages\\customPic.png")]
        public string CustomPic {
            get {
                return ((string)(this["CustomPic"]));
            }
            set {
                this["CustomPic"] = value;
            }
        }
    }
}
