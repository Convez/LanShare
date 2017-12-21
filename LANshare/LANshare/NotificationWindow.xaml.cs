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
using LANshare.Model;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        private string _message;
        private Notification.NotificationType _notificationType;
        private int _notificationsInQueue;

        /// <summary>
        /// Called when the notification is clicked. provides the messageType
        /// </summary>
        public NotificationWindow( Notification n , int notificationsInQueue)
        {
            InitializeComponent();
            DataContext = this;
            Title.Text = n.Title;
            _notificationType= n.MsgType;
            _notificationsInQueue = notificationsInQueue;
            switch((int)_notificationType)
            {
                case 0:
                    {
                        _message = "File transfer pending request from " + n.SubjectOfNotification+".";
                        break;
                    }
                case 1:
                    {
                        _message = "File transfer with user " + n.SubjectOfNotification + " has completed successfully.";
                        break;
                    }
                case 2:
                    {
                        _message= "File transfer with user " + n.SubjectOfNotification + " has been aborted.\nClick here for more info.";
                        break;
                    }
                case 3:
                    {
                        _message = n.SubjectOfNotification + " is online.";
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
            if(_notificationsInQueue>0)
            {
                Hide();
                //change the icon
            }else   Close();
        }


        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch ((int)_notificationType)
            {
                case 0:
                    {
                        //open window of filetransfers with transfer request details on top or a special window 
                        int w = Application.Current.Windows.OfType<TransfersWindow>().Count();
                        if (w > 0)
                        {
                            //call a function in transfers window to show details of notification object
                            Application.Current.Windows.OfType<TransfersWindow>().First().Activate();
                        }
                        else if (w == 0)
                        {
                            TransfersWindow window = new TransfersWindow();
                            //call a function in transfers window to show details of notification object
                            window.Show();
                        }
                        break;
                    }
                case 1:
                    {
                        //open filetransfers window
                        int w = Application.Current.Windows.OfType<TransfersWindow>().Count();
                        if (w > 0)
                        {
                            //call a function in transfers window to show details of notification object
                            Application.Current.Windows.OfType<TransfersWindow>().First().Activate();
                        }
                        else if (w == 0)
                        {
                            TransfersWindow window = new TransfersWindow();
                            //call a function in transfers window to show details of notification object
                            window.Show();
                        }
                        break;
                    }
                case 2:
                    {
                        //open window of filetransfers with transfer abort details on top or a special window
                        int w = Application.Current.Windows.OfType<TransfersWindow>().Count();
                        if (w > 0)
                        {
                            //call a function from transfers window that shows abort details
                            Application.Current.Windows.OfType<TransfersWindow>().First().Activate();
                        }
                        else if (w == 0)
                        {
                            TransfersWindow window = new TransfersWindow();
                            //call a function from transfers window that shows abort details
                            window.Show();
                        }
                        break;
                    }
                case 3:
                    {
                        //open users online window
                        int w = Application.Current.Windows.OfType<ShowUsersWindow>().Count();
                        if (w > 0)
                        {
                            Application.Current.Windows.OfType<ShowUsersWindow>().First().Activate();
                        }
                        else if (w == 0)
                        {
                            ShowUsersWindow window = new ShowUsersWindow();
                            window.Show();
                        }

                        break;
                    }
            }
            Close();

        }

        public void NewNotificationInQueue()
        {
            _notificationsInQueue++;
        }
    }
}
