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
using System.Net.Cache;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, ListWindow<User>
    {

        public event EventHandler peopleButtonClick;
        public event EventHandler transfersButtonClick;
        public event EventHandler settingsButtonClick;
        public event EventHandler privacyChanged;

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
                privacyChanged?.Invoke(this, null);
            }
        }

        private void GetPicFromFIle()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1;
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" +"All files (*.*)|*.*";
            openFileDialog1.Title = "Select Profile Picture";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)

            {

                String file = openFileDialog1.FileName;

                try

                {
                    Configuration.CurrentUser.ProfilePicture = null;
                    System.IO.File.Copy(file, Properties.Settings.Default.CustomPic, true);
                    BitmapImage profile_pic = new BitmapImage();
                    //profile_pic.BeginInit();
                    //profile_pic.CacheOption = BitmapCacheOption.OnLoad;

                    //profile_pic.UriSource= new Uri(LANshare.Properties.Settings.Default.CustomPic, UriKind.Relative); 

                    //profile_pic.EndInit();
                    profile_pic.BeginInit();
                    profile_pic.CacheOption = BitmapCacheOption.None;
                    profile_pic.UriCachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.BypassCache);
                    profile_pic.CacheOption = BitmapCacheOption.OnLoad;
                    profile_pic.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    profile_pic.UriSource = new Uri(LANshare.Properties.Settings.Default.CustomPic, UriKind.Relative);
                    profile_pic.EndInit();

                    Configuration.CurrentUser.ProfilePicture = profile_pic;
                    Configuration.SaveConfiguration();
                    //this.FindResource("profileImg");
                    this.UpdateLayout();
                    
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

        private void OnUsersClick(object sender, RoutedEventArgs e)
        {
            OnPeopleWindowSelected();

        }

        private void OnTransfersClick(object sender, RoutedEventArgs e)
        {
            OnTransferWindowSelected();
        }

        public void setList(List<User> list)
        {
            //does nothing on pupose
        }
        

        public void OnPeopleWindowSelected()
        {
            peopleButtonClick?.Invoke(this, null);

        }

        public void OnSettingWindowSelected()
        {
            //settingsButtonClick?.Invoke(this, null);

        }

        public void OnTransferWindowSelected()
        {
            transfersButtonClick?.Invoke(this, null);
        }
    }
}
