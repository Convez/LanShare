using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace LANshare.Model
{
    [Serializable]
    public class User 
    {
        public string Name
        {
            get => _name;
            set => _name = Environment.ExpandEnvironmentVariables(value);
        }
        private string _name;
        public string NickName { get; set; }
<<<<<<< HEAD
<<<<<<< HEAD

        //Session Id
        public string SessionId { get; set; }

=======
        public bool online;
>>>>>>> 6c12d19fed146f384b9d839380d70a39b9bb7905
=======
        public bool online;
>>>>>>> 6c12d19fed146f384b9d839380d70a39b9bb7905
        //User ip address
        [NonSerialized] public System.Net.IPAddress userAddress;
        // Tcp port listening for file upload requests for user
        public int TcpPortTo { get; set; }
        public User(string name, int tcpPortTo , string nickName=null)
        {
            Name = name;
            TcpPortTo = tcpPortTo;
            NickName = nickName;
            online=true;
        }
        
    }
}
