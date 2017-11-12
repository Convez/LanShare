using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
<<<<<<< HEAD
=======
using System.Security.Cryptography;
using System.Threading;
>>>>>>> Enrico_TB

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
=======
        //Session Id
        public object SessionId { get=>_sessionId; set=>Interlocked.Exchange(ref _sessionId,value); }

        private object _sessionId;
>>>>>>> Enrico_TB
        //User ip address
        [NonSerialized] public System.Net.IPAddress userAddress;
        // Tcp port listening for file upload requests for user
        public int TcpPortTo { get; set; }
        public User(string name, int tcpPortTo , string nickName=null)
        {
            Name = name;
            TcpPortTo = tcpPortTo;
            NickName = nickName;
        }
<<<<<<< HEAD
        
=======

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
>>>>>>> Enrico_TB
    }
}
