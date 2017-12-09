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
using System.Drawing;
using LANshare.Model;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        
        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = Model.Configuration.CurrentUser;

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
            else    WindowState = WindowState.Maximized;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            switch(b.Name)
            {
                case "EditNickButton":
                    InputWindow i = new InputWindow("Choose your nickname:");
                    if (i.ShowDialog() == true)
                    {
                        Configuration.CurrentUser.NickName = i.Input;
                    }
                    break;
                case "EditPrivButton":
                    Configuration.CurrentUser.SetPrivacyMode();
                    break;
                case "EditNicButton":
                    //make chose the file
                    break;
            }

            
                
                
        }
    }
}
