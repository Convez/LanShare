using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANshare.Model
{
    [Serializable]
    public class User 
    {
        public string Name
        {
            get { return this.Name; }
            set {this.Name = Environment.ExpandEnvironmentVariables(value); }
        }
        public string NickName { get; set; }
        public User(string name,string nickName=null)
        {
            Name = name;
            NickName = nickName;
        }
    }
}
