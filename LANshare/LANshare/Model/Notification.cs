using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANshare.Model
{
    public class Notification
    {
        private NotificationType _messageType;
        private String _title;
        private String _subjectOfNotification;
        public enum NotificationType { transferRequest, transferCompletion, transferAbort, userOnline } //teniamo userOnline? va trattato diversaente come notifica..

        public Notification( string title, NotificationType messageType, String subjectOfNotification)
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
        public string SubjectOfNotification
        {
            get => _subjectOfNotification;
        }

    }
}
