using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using LANshare.Connection;
using LANshare.Model;
using LANshare.Properties;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace LANshare
{
    class NotificationManager
    {

        private string _message;
        private Notification.NotificationType _notificationType;
        private int _notificationsInQueue;

        /// <summary>
        /// Called when the notification is clicked. provides the messageType
        /// </summary>
        public NotificationManager(Notification n, int notificationsInQueue)
        {

            _notificationType = n.MsgType;
            _notificationsInQueue = notificationsInQueue;
            switch ((int)_notificationType)
            {
                case 0:
                    {
                        _message = "File transfer pending request from " + n.SubjectOfNotification + ".";
                        break;
                    }
                case 1:
                    {
                        _message = "File transfer with user " + n.SubjectOfNotification + " has completed successfully.";
                        break;
                    }
                case 2:
                    {
                        _message = "File transfer with user " + n.SubjectOfNotification + " has been aborted.\nClick here for more info.";
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
            //Close();

        }

        public void NewNotificationInQueue()
        {
            _notificationsInQueue++;
        }
    }
}
