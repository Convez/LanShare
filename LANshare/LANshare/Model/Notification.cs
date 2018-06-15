using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANshare.Model
{
    public class Notification
    {
        private NotificationType _messageType;
        private String _title;
        private User _subjectOfNotification;


        public enum NotificationType { transferRequest, transferCompletion, transferAbort, userOnline } //teniamo userOnline? va trattato diversaente come notifica..

        public Notification( string title, NotificationType messageType, User subjectOfNotification)
        {
            _messageType = messageType;
            _title = title;
            _subjectOfNotification = subjectOfNotification;
        }

        public NotificationType MsgType
        {
            get=> _messageType;
        }
        public string Title
        {
            get => _title;
        }
        public User SubjectOfNotification
        {
            get => _subjectOfNotification;
        }



    }
}
