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
        protected String name;
        protected String nickname;
        
        public SettingsWindow()
        {
            InitializeComponent();
            Bitmap img;
            //ImageSource i = new ImageSource(LANshare.Properties.Resources.personalPic);

            //if (!LANshare.Properties.Settings.Default.PersonalPic) img = new Bitmap(LANshare.Properties.Resources.defaultPic);
            //else img = LANshare.Properties.Resources.personalPic;

            //ImageSource i=

            name=Model.Configuration.CurrentUser.Name;
            nickname = Model.Configuration.CurrentUser.NickName;
            Console.WriteLine(nickname);
            User localuser = Model.Configuration.CurrentUser;

            this.DataContext = localuser;

            

        }
        
    }
}
