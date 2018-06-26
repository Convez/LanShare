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
            //u = new User("gigino",3,EUserAdvertisementMode.Public,null,null);
       
            //u.ProfilePicture = new BitmapImage(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Configuration.DefaultPicPath, UriKind.Absolute));
            //transf = new FileDownloadHelper();
            //transf.Counterpart = u;
            //transf.Status = TransferCompletitionStatus.Sending;
            //FileTransferProgressChangedArgs args = new FileTransferProgressChangedArgs(10,10,50, new TimeSpan(0,5,45));
            //transf.Args=args;
            //transf.FileName = "verylongfilenamethatjustdoesntend123456789youwishitwasover.jpg";
            //AddTransfer(this, transf);
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

            foreach (ListViewItem item in ActiveTransfers.Items)
            {
                if (item.Content.Equals("ciao"))
                {

                }
            }

        }

        public void RemoveTransfer(object sender, IFileTransferHelper t)
        {
            ActiveTransfers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    transfersList.Remove(t);

                    foreach (ListViewItem item in ActiveTransfers.Items)
                    {
                        if (item.Content.Equals("ciao"))
                        {
                            
                        }
                    }
                    
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
            ConfirmationWindow C = new ConfirmationWindow("The file transfer will be aborted, continue?");
            C.ShowDialog();
            if (C.DialogResult == true)
            {
                Button b= (Button)sender;
                IFileTransferHelper t = b.DataContext as IFileTransferHelper;
                t.Cancel();
                
            }
            
        }

        
    }
}
