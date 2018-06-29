using System;
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

namespace LANshare
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window , INotifyPropertyChanged
    {
        private String _defaultText = "Type here";
        private String _input = null;
        private String _hint = null;
        private int _maxLength = 20;
        public event PropertyChangedEventHandler PropertyChanged;
        public InputWindow( String title  )
        {
            InitializeComponent();
            Title.Text = title;
            InputField.Text = _defaultText;
            DataContext = this;
            
        }

        private void SubmitInput(object sender, RoutedEventArgs e)
        {
            _input = null;
            _input = !InputField.Text.Equals(_defaultText)?InputField.Text:string.Empty;
            if (_input == String.Empty)
            {
                this.DialogResult = false;
            }
            else
            {
                if (_input.Length > _maxLength)
                {
                    _hint = "Nickname can have at most " + _maxLength + " characters.";
                    OnPropertyChanged("Hint");
                    InputField.Text = string.Empty;
                    _input = null;
                    return;
                }
                if (!Regex.IsMatch(_input, @"^[a-zA-Z0-9'.\s]{1," + _maxLength + "}$"))
                {
                    _hint = "Nickname can have at most " + _maxLength + " characters. \nAllowed: a-Z , A-Z, 0-9 and whitespace.";
                    OnPropertyChanged("Hint");
                    InputField.Text = string.Empty;
                    _input = null;
                    return;
                }
                this.DialogResult = true;
            }
           

        }

        //when textbox is selected it is cleared along wiht hint field
        public void TextBox_GotFocus(object sender, RoutedEventArgs e) 
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            _hint = string.Empty;
            OnPropertyChanged("Hint");
            //tb.GotFocus -= TextBox_GotFocus;
        }

        public String Input
        {
            get => _input;
        }

        public String Hint
        {
            get => _hint;
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

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
    }
}
