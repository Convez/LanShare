﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class NotificationWindow : LanShareWindow
    {


        private string _message;
        private ObservableCollection<Notification> notificationList = new ObservableCollection<Notification>();

        /// <summary>
        /// Called when the notification is clicked. provides the messageType
        /// </summary>
        public NotificationWindow(ObservableCollection<Notification> nList)
        {
            InitializeComponent();

            notificationList = nList;
            NotificationsList.ItemsSource = notificationList;
            DataContext = this;

        }
        public String Message
        {
            get => _message;
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

       
    }
}
