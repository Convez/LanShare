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

        private IFileTransferHelper transf;
        private User u;

        public TransfersWindow()
        {
            InitializeComponent();
            transfersList = new ObservableCollection<IFileTransferHelper>();
            ActiveTransfers.Items.Clear();
            ActiveTransfers.ItemsSource = transfersList;
            //FileUploadHelper fu = new FileUploadHelper();
            //User u = new User("prova", 111, EUserAdvertisementMode.Public);
            //List<string> files= new List<string>("uno")
            //fu.InitFileSend(u,)
            //transfersList.Add()
            u = new User("gigino",3,EUserAdvertisementMode.Public,null,null);
       
            u.ProfilePicture = new BitmapImage(new Uri(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + Configuration.DefaultPicPath, UriKind.Absolute));
            transf = new FileDownloadHelper();
            transf.Counterpart = u;
            transf.Status = TransferCompletitionStatus.Sending;
            transf.Percentage = 40;
            AddTransfer(this, transf);
        }



        public void AddTransfer(object sender, IFileTransferHelper t)
        {
            ActiveTransfers.Dispatcher.Invoke(() =>
            {
                lock (l)
                {
                    transfersList.Add(t);
                    t.TransferCompleted += (o, a) => UpdateStatus(a, t);
                }
            });
        }

        private void UpdateStatus(TransferCompletitionStatus status,IFileTransferHelper t)
        {

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

        
    }
}
