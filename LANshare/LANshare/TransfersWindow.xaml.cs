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

namespace LANshare
{
    /// <summary>
    /// Interaction logic for TransfersWindow.xaml
    /// </summary>
    public partial class TransfersWindow : Window
    {

        public TransfersWindow()
        {
            InitializeComponent();
        }



        public void AddTransfer(object sender, Transfer t)
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
            TrayIconWindow.OpenWindow<SettingsWindow>();

        }

        private void OnUsersClick(object sender, RoutedEventArgs e)
        {
            TrayIconWindow.OpenWindow<ShowUsersWindow>();

        }

    }
}
