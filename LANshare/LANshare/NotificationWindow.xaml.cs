using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private string _message;
        private int _messageType;

        public enum MessageType { transferRequest, transferCompletion, transferAbort, userOnline }
        public NotificationWindow( string title, MessageType messageType , string subjectOfNotification )
        {
            InitializeComponent();
            DataContext = this;
            Title.Text = title;
            _messageType= (int)messageType;
            switch(_messageType)
            {
                case 0:
                    {
                        _message = "File transfer pending request from " + subjectOfNotification+".";
                        break;
                    }
                case 1:
                    {
                        _message = "File transfer with user " + subjectOfNotification + " has completed successfully.";
                        break;
                    }
                case 2:
                    {
                        _message= "File transfer with user " + subjectOfNotification + " has been aborted.\nClick here for more info.";
                        break;
                    }
                case 3:
                    {
                        _message = subjectOfNotification + " is online.";
                        break;
                    }
            }
        }
        public String Message
        {
            get => _message;
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseClick(object sender, MouseButtonEventArgs e)
        {
            

        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (_messageType)
            {
                case 0:
                    {
                        //open window of filetransfers with transfer request details on top or a special window 
                        break;
                    }
                case 1:
                    {
                        //open filetransfers window
                        break;
                    }
                case 2:
                    {
                        //open window of filetransfers with transfer abort details on top or a special window 
                        break;
                    }
                case 3:
                    {
                        //open users online window
                        break;
                    }
            }
        }
    }
}
