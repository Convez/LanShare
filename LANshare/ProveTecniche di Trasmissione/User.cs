using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LANshare.Model
{
    [Serializable]
    public class User
    {
        public string name { get; set; }
        

        public string surname { get; set; }
        

        public override string ToString()
        {
            return name + " " + surname;
        }
    }
}
