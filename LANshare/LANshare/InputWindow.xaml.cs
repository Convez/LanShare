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

namespace LANshare
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        private String _defaultText = "Type here";
        private String _input = null;
        public InputWindow( String title  )
        {
            InitializeComponent();
            Title.Text = title;
            InputField.Text = _defaultText;
        }

        private void SubmitInput(object sender, RoutedEventArgs e)
        {
            _input = null;
            _input = InputField.Text;
            if (_input == null) this.DialogResult = false;
            else
            {

            }
            this.DialogResult = true;

        }
        public void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TextBox_GotFocus;
        }

        public String Input
        {
            get => _input;
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            _input = null;
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

    }
}
