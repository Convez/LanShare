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
    public partial class ShowUsersWindowDLL : Window,ListWindow<User>
    {

        private readonly ObservableCollection<User> userList;
        private readonly object l = "";

        public event EventHandler<SendEventArgs> UsersSelected;
        private bool _isSubscribed = false;
        public Boolean isSubscribed
        {
            get => _isSubscribed;
            set => _isSubscribed=value;
        }

        public event EventHandler peopleButtonClick;
        public event EventHandler transfersButtonClick;
        public event EventHandler settingsButtonClick;

        public ShowUsersWindowDLL()
        {
            InitializeComponent();
            userList = new ObservableCollection<User>();
            ConnectedUsers.ItemsSource = userList;
            
        }
        public ShowUsersWindowDLL(List<User> starting)
        {
            InitializeComponent();
            userList = new ObservableCollection<User>(starting);
            ConnectedUsers.ItemsSource = userList;
            
        }
        
        private void SendButtonClicked(object sender, RoutedEventArgs args)
        {
            List<User> selectedUsers = ConnectedUsers.SelectedItems.OfType<User>().Distinct().ToList();
            if (selectedUsers.Count <= 0)
            {
                NotificationWindow C = new NotificationWindow("Please, select at least one user.");
                C.ShowDialog();
            }
            else
            {

                Button b = (Button)sender;
                if(b.Name=="File")  OnUsersSelected(selectedUsers, "file");
                else if(b.Name=="Folder") OnUsersSelected(selectedUsers, "folder");
                else OnUsersSelected(selectedUsers, "dll");
                //Close();
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

        protected virtual void OnUsersSelected(List<User> e, string mode) //mode is "file" if file must be sent, "folder" if folder
        {
            UsersSelected?.Invoke(this, new SendEventArgs(e, mode ));
        }


        public void setList(List<User> list)
        {
            userList.Clear();
            list.ForEach(userList.Add);
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            OnSettingWindowSelected();

        }

        private void OnTransfersClick(object sender, RoutedEventArgs e)
        {
            OnTransferWindowSelected();
        }


        public void OnPeopleWindowSelected()
        {
            //peopleButtonClick?.Invoke(this, null);

        }

        public void OnSettingWindowSelected()
        {
            settingsButtonClick?.Invoke(this, null);

        }

        public void OnTransferWindowSelected()
        {
            transfersButtonClick?.Invoke(this, null);
        }

        private void ListViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            List<User> l = new List<User>();
            ListViewItem u = (ListViewItem)sender;
            object x = u.Content;
            l.Add((User)x);
            OnUsersSelected(l, "file");
        }

        public class SendEventArgs : EventArgs
        {
            public SendEventArgs(params object[] args)
            {
                Args = args;
            }

            public object[] Args { get; set; }
        }

        //private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    //ContextMenu cm = new ContextMenu();
        //    //Button b = new Button();

        //    //cm.IsOpen = true;
        //}
    }
}
