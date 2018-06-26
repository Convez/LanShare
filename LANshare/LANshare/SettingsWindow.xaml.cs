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
using System.ComponentModel;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, ListWindow<User>, INotifyPropertyChanged
    {

        public event EventHandler peopleButtonClick;
        public event EventHandler transfersButtonClick;
        public event EventHandler settingsButtonClick;
        public event EventHandler privacyChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isSubscribed;
        public Boolean isSubscribed
        {
            get => _isSubscribed;
            set => _isSubscribed = value;
        }
        private string _savePath;
        public User User
        {
            get => Configuration.CurrentUser;
            
        }

        private Visibility _visibility = Visibility.Collapsed;
        public Visibility PathAAVisibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                OnPropertyChanged("PathAAVisibility");
            }

        }
        public string SavePath { get => _savePath;
            set
            {
                _savePath = value;
                OnPropertyChanged("SavePath");
            }
        }

        private string _savePathMode;

        public string SavePathMode
        {
            get => _savePathMode;
            set
            {
                _savePathMode = value;
                if (value == "Use Custom")
                {
                    Configuration.FileSavePathMode = EFileSavePathMode.UseCustom;
                    if (String.IsNullOrWhiteSpace(Configuration.CustomSavePath)) SavePath = Configuration.DefaultSavePath;
                    else SavePath = Configuration.CustomSavePath;
                    var editPath = (System.Windows.Controls.Button)this.FindName("editPathButton");
                    editPath.Visibility = Visibility.Visible;
                    var pathField = (System.Windows.Controls.TextBlock)this.FindName("pathView");
                    pathField.Visibility = Visibility.Visible;
                    var pathLabel = (System.Windows.Controls.TextBlock)this.FindName("pathLabel");
                    pathLabel.Visibility = Visibility.Visible;
                }else if (value=="Use Default")
                {
                    Configuration.FileSavePathMode = EFileSavePathMode.UseDefault;
                    SavePath = Configuration.DefaultSavePath;
                    var editPath = (System.Windows.Controls.Button)this.FindName("editPathButton");
                    editPath.Visibility = Visibility.Collapsed;
                    var pathField = (System.Windows.Controls.TextBlock)this.FindName("pathView");
                    pathField.Visibility = Visibility.Visible;
                    var pathLabel = (System.Windows.Controls.TextBlock)this.FindName("pathLabel");
                    pathLabel.Visibility = Visibility.Visible;
                }
                else if (value=="Ask Always")
                {
                    Configuration.FileSavePathMode = EFileSavePathMode.AskForPath;
                    var editPath = (System.Windows.Controls.Button)this.FindName("editPathButton");
                    editPath.Visibility = Visibility.Collapsed;
                    var pathField = (System.Windows.Controls.TextBlock)this.FindName("pathView");
                    pathField.Visibility = Visibility.Collapsed;
                    var pathLabel = (System.Windows.Controls.TextBlock)this.FindName("pathLabel");
                    pathLabel.Visibility = Visibility.Collapsed;
                }
                OnPropertyChanged("SavePathMode");
            }
        }

        private string _acceptanceMode;
        public string AcceptanceMode
        {
            get => _acceptanceMode;
            set
            {
                _acceptanceMode = value;
                OnPropertyChanged("AcceptanceMode");
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            if(!String.IsNullOrWhiteSpace(Configuration.CustomSavePath) && Configuration.FileSavePathMode == EFileSavePathMode.UseCustom)
            {
                SavePathMode = "Use Custom";
            }
            else if (Configuration.FileSavePathMode == EFileSavePathMode.UseDefault)
            {
                SavePathMode = "Use Default";
            }
            else
            {
                SavePathMode = "Ask Always";
            }

            if (Configuration.FileAcceptanceMode == EFileAcceptanceMode.AcceptAll)
            {
                AcceptanceMode = "Accept Automatically";

                System.Windows.Controls.ContextMenu m = EditSaveButton.ContextMenu;
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)m.Items[2];
                mi.Visibility = Visibility.Collapsed;

            }
            else
            {
                AcceptanceMode = "Ask Always";
                System.Windows.Controls.ContextMenu m = EditSaveButton.ContextMenu;
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)m.Items[2];
                mi.Visibility = Visibility.Visible;
            }
            DataContext = this;
            
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

       

        private void EditNick(object sender, RoutedEventArgs e)
        {
            InputWindow i = new InputWindow("Choose your nickname:");
            i.ShowDialog();
            if (i.DialogResult == true)
            {
                Configuration.CurrentUser.NickName = i.Input;
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

        private void AcceptanceSetter(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem m = (System.Windows.Controls.MenuItem)sender;
            if (m.Header.ToString() =="Ask Always")
            {
                AcceptanceMode = "Ask Always";
                System.Windows.Controls.ContextMenu m2 = EditSaveButton.ContextMenu;
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)m2.Items[2];
                mi.Visibility = Visibility.Visible;
                Configuration.FileAcceptanceMode = EFileAcceptanceMode.AskAlways;
            }
            else if(m.Header.ToString()== "Accept Automatically")
            {
                AcceptanceMode = "Accept Automatically";
                System.Windows.Controls.ContextMenu m2 = EditSaveButton.ContextMenu;
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)m2.Items[2];
                mi.Visibility = Visibility.Collapsed;
                Configuration.FileAcceptanceMode = EFileAcceptanceMode.AcceptAll;
                if (SavePathMode == "Ask Always") {
                    if (String.IsNullOrWhiteSpace(Configuration.CustomSavePath))
                    {
                        SavePathMode = "Use Default";
                        
                    }
                    else
                    {
                        SavePathMode = "Use Custom";
                        
                    }
                } 
            }
        }
        private void PathSetter(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem m = (System.Windows.Controls.MenuItem)sender;
            SavePathMode = m.Header.ToString();
              
            Configuration.SaveConfiguration();
        }

        private void EditPath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog;
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select Your File Save Path";

            DialogResult dr = folderBrowserDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)

            {
                string path = folderBrowserDialog.SelectedPath;
                Configuration.CustomSavePath = path;
                Configuration.SaveConfiguration();
                SavePath = path;
            }

                
        }


        private void EditPicture(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1;
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" +"All files (*.*)|*.*";
            openFileDialog1.Title = "Select Your Profile Picture";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)

            {

                String file = openFileDialog1.FileName;

                try

                {
                    //Configuration.CurrentUser.ProfilePicture = null;
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
        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }



    }
}
