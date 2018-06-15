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
    /// Interaction logic for LanShareWindow.xaml
    /// </summary>
    public partial class LanShareWindow : Window
    {
        
        private ObservableCollection<IFileTransferHelper> transfersList = new ObservableCollection<IFileTransferHelper>();

        public event EventHandler peopleButtonClick;
        public event EventHandler transfersButtonClick;
        public event EventHandler settingsButtonClick;
        public event EventHandler notificationsButtonClick;

        public LanShareWindow()
        {
            InitializeComponent();
            
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
            transfersButtonClick?.Invoke(this, null);

        }

        public void OnNotificationsWindowSelected()
        {
            notificationsButtonClick?.Invoke(this, null);

        }


    }
}
