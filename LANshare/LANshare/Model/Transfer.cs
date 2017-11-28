﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANshare.Model
{
    public class Transfer
    {
        private User _user;
        private bool _sending; //true= files are being sent , false= files are being received
        private int _percentage;
        
        public User OtherUser
        {
            get => _user;
        }
        public String OtherUserName
        {
            get => _user.Name;
        }
        public bool IsSending
        {
            get => _sending;
        }
        public int Percentage
        {
            get => _percentage;
            set => _percentage = value;
        }

        public Transfer(User u, bool sending , object[] args )
        {
            _user = u;
            _sending = sending;

        }
        

    }
}
