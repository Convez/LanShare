using System.Collections.Generic;
using System.ComponentModel;
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
using System.Text.RegularExpressions;
using System;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for ConfirmationWindow.xaml
    /// </summary>
    public partial class ConfirmationWindow : Window
    {

        public ConfirmationWindow(String title)
        {
            InitializeComponent();
            Title.Text = title;
            DataContext = this;
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            
                this.DialogResult = true;
          
        }

        private void Abort(object sender, RoutedEventArgs e)
        {

            this.DialogResult = false;

        }



        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

       
    }
}
