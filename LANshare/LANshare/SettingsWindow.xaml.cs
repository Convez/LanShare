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
using System.Diagnostics;
using System.Windows.Forms;

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
            System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
            switch(b.Name)
            {
                case "EditNickButton":
                    InputWindow i = new InputWindow("Choose your nickname:");
                    i.ShowDialog();
                    if (i.DialogResult == true)
                    {
                        Configuration.CurrentUser.NickName = i.Input;
                    }
                    break;
                case "EditPicButton":
                    //make chose the file
                    GetPicFromFIle();
                    break;
                
            }    
        }
        private void PrivacySetter(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem m = (System.Windows.Controls.MenuItem)sender;
            if (m.Header.ToString() != Configuration.CurrentUser.PrivacyMode)
            {
                Configuration.CurrentUser.SetPrivacyMode();
            }
        }

        private void GetPicFromFIle()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1;
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.ShowDialog();
            openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" +"All files (*.*)|*.*";


            openFileDialog1.Title = "Select Profile Picture";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)

            {

                String file = openFileDialog1.FileName;

                try

                {
                    System.IO.File.Copy(file, "Media/Images/UserImages/" + Properties.Settings.Default.CustomPic , true);
                    BitmapImage profile_pic = new BitmapImage(new Uri(LANshare.Properties.Settings.Default.DefaultPic, UriKind.Relative));
                    Configuration.CurrentUser.ProfilePicture = profile_pic;
                }

                catch (Exception ex)

                {

                    System.Windows.MessageBox.Show("Error: " + ex.Message);

                }
            }
        }

        private bool myCallback()
        {
            throw new NotImplementedException();
        }
    }
}
