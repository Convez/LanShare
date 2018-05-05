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

namespace LANshare
{
    /// <summary>
    /// Interaction logic for TransfersWindow.xaml
    /// </summary>
    public partial class TransfersWindow : Window, ListWindow<IFileTransferHelper>
    {
        private List<IFileTransferHelper> transfersList=new List<IFileTransferHelper>() ;

        public event EventHandler peopleButtonClick;
        public event EventHandler transfersButtonClick;
        public event EventHandler settingsButtonClick;

        public TransfersWindow()
        {
            InitializeComponent();
        }



        public void AddTransfer(object sender, IFileTransferHelper t)
        {
            transfersList.Add(t);
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
            list.ForEach(transfersList.Add);

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
