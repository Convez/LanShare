using LANshare.Connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using LANshare.Model;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for ShowUsersWindow.xaml
    /// </summary>
    public partial class ShowUsersWindow : Window
    {
        private LanComunication _comunication;
        private Task _UDPlistener;
        private CancellationTokenSource _cts;

        private ObservableCollection<User> userList;
        private readonly object l = "";
        public ShowUsersWindow()
        {
            InitializeComponent();
            userList = new ObservableCollection<User>();
            ConnectedUsers.ItemsSource = userList;
        }

        public void AddUser(object sender, User u)
        {
            ConnectedUsers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    userList.Add(u);
                }
            });
        }

        public void RemoveUsers(object sender, List<User> li)
        {
            ConnectedUsers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    li.ForEach(u => userList.Remove(u));
                }
            });
        }
        protected override void OnClosed(EventArgs e) => base.OnClosed(e);

    }
}
