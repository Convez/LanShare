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
using LANshare.Connection;
using System.Collections.ObjectModel;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for TransfersWindow.xaml
    /// </summary>
    public partial class TransfersWindow : Window, ListWindow<IFileTransferHelper>
    {
        private ObservableCollection<IFileTransferHelper> transfersList=new ObservableCollection<IFileTransferHelper>() ;

        public event EventHandler peopleButtonClick;
        public event EventHandler transfersButtonClick;
        public event EventHandler settingsButtonClick;
        private readonly object l = "";
        private bool _isSubscribed;
        public Boolean isSubscribed
        {
            get => _isSubscribed;
            set => _isSubscribed = value;
        }
        private IFileTransferHelper transf;
        private User u;

        public TransfersWindow()
        {
            InitializeComponent();
            transfersList = new ObservableCollection<IFileTransferHelper>();
            ActiveTransfers.ItemsSource = transfersList;
            //FileUploadHelper fu = new FileUploadHelper();
            //User u = new User("prova", 111, EUserAdvertisementMode.Public);
            //List<string> files= new List<string>("uno")
            //fu.InitFileSend(u,)
            //transfersList.Add()
            u = new User("gigino", 3, EUserAdvertisementMode.Public, null, null);

            u.ProfilePicture = new BitmapImage(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Configuration.DefaultPicPath, UriKind.Absolute));
            transf = new FileDownloadHelper();
            transf.Counterpart = u;
            transf.Status = TransferCompletitionStatus.Sending;
            FileTransferProgressChangedArgs args = new FileTransferProgressChangedArgs(10, 10, 50, new TimeSpan(0, 5, 45));
            transf.Args = args;
            transf.FileName = "verylongfilenamethatjustdoesntend123456789youwishitwasover.jpg";
            AddTransfer(this, transf);
        }



        public void AddTransfer(object sender, IFileTransferHelper t)
        {
            ActiveTransfers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    transfersList.Add(t);
                  
                }
            });

            

        }

        public void RemoveTransfer(object sender, IFileTransferHelper t)
        {
            ActiveTransfers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    transfersList.Remove(t);

                    
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

        private void OnSettingClick(object sender, RoutedEventArgs e)
        {
            OnSettingWindowSelected();
        }

        private void OnUsersClick(object sender, RoutedEventArgs e)
        {
            OnPeopleWindowSelected();
        }

        
        public void setList(List<IFileTransferHelper> list)
        {
            ActiveTransfers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    list.ForEach(t => transfersList.Add(t));
                }
            });

            //list.ForEach(transfersList.Add);

        }

        public void OnPeopleWindowSelected()
        {
            peopleButtonClick?.Invoke(this, null);
            //foreach ( object item in ActiveTransfers.Items)
            //{
            //    ListViewItem l = (ListViewItem)ActiveTransfers.ItemContainerGenerator.ContainerFromItem(item);
            //    DependencyObject o = VisualTreeHelper.GetChild(l, 0);
            //    int i = VisualTreeHelper.GetChildrenCount(o);
            //    DependencyObject o1 = VisualTreeHelper.GetChild(o, 0);
            //    int j = VisualTreeHelper.GetChildrenCount(o1);

            //    DependencyObject o2 = VisualTreeHelper.GetChild(o1, 0);
            //    DependencyObject o3 = VisualTreeHelper.GetChild(o2, 0);
            //    int ji = VisualTreeHelper.GetChildrenCount(o3);

            //    DependencyObject o4 = VisualTreeHelper.GetChild(o3, 6);
            //    DependencyObject o5 = VisualTreeHelper.GetChild(o4, 0);
            //    ProgressBar progressBar = (ProgressBar)o5;
                
                
            //}
        }

        public void OnSettingWindowSelected()
        {
            settingsButtonClick?.Invoke(this, null);

        }

        public void OnTransferWindowSelected()
        {
            //transfersButtonClick?.Invoke(this, null);

        }

        public void AbortTransfer(object sender, RoutedEventArgs e) {
            Button b = (Button)sender;
            IFileTransferHelper t = b.DataContext as IFileTransferHelper;
            if(t.Status!= TransferCompletitionStatus.Refused && t.Status!=TransferCompletitionStatus.Canceled && t.Status!= TransferCompletitionStatus.Completed && t.Status!= TransferCompletitionStatus.Error)
            {
                ConfirmationWindow C = new ConfirmationWindow("The file transfer will be aborted, continue?");
                C.ShowDialog();
                if (C.DialogResult == true)
                {
                    Task.Run(() => t.Cancel());
                    RemoveTransfer(null, t);
                }
            }
            else
            {
                RemoveTransfer(null, t);
            }
           
            
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            List<IFileTransferHelper> selectedUsers = ActiveTransfers.SelectedItems.OfType<IFileTransferHelper>().ToList();
            if (selectedUsers.Count <= 0)
            {
                List<IFileTransferHelper> temp =new List<IFileTransferHelper>() ;
                foreach (IFileTransferHelper t in transfersList)
                {
                    if (t.Status == TransferCompletitionStatus.Canceled || t.Status == TransferCompletitionStatus.Error || t.Status == TransferCompletitionStatus.Completed || t.Status == TransferCompletitionStatus.Refused)
                        temp.Add(t);
                }
                foreach (IFileTransferHelper t in temp)
                {
                    if (t.Status == TransferCompletitionStatus.Canceled || t.Status == TransferCompletitionStatus.Error || t.Status == TransferCompletitionStatus.Completed || t.Status == TransferCompletitionStatus.Refused)
                        transfersList.Remove(t);
                }
            }
            else
            {
                foreach(IFileTransferHelper t in selectedUsers)
                {
                    if(t.Status==TransferCompletitionStatus.Canceled || t.Status == TransferCompletitionStatus.Error || t.Status == TransferCompletitionStatus.Completed || t.Status == TransferCompletitionStatus.Refused)
                        transfersList.Remove(t);
                }
                //Close();
            }
        }
    }
}
