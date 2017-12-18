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

        public event EventHandler<List<User>> usersSelected;
        public ShowUsersWindow()
        {
            InitializeComponent();
            userList = new ObservableCollection<User>();
            ConnectedUsers.ItemsSource = userList;
            SendButton.Click += SendButtonClicked;
        }
        public ShowUsersWindow(List<User> starting)
        {
            InitializeComponent();
            userList = new ObservableCollection<User>(starting);
            ConnectedUsers.ItemsSource = userList;
            SendButton.Click += SendButtonClicked;
        }
        private void SendButtonClicked(object sender,EventArgs args)
        {
            List<User> selectedUsers = ConnectedUsers.SelectedItems.OfType<User>().ToList();
            if (selectedUsers.Count <= 0)
            {
                MessageBox.Show("Select at least one user");
            }
            else
            {
                usersSelected?.Invoke(this, selectedUsers);
                Close();
            }
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

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Maximize_Button_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

    }
}
