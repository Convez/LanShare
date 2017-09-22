using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LANshare
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Model.Configuration.LoadConfiguration();
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                var userWindow = new ShowUsersWindow();
                userWindow.Show();
            }
            else
            {
                var trayIconWindow = new TrayIconWindow();
                trayIconWindow.Activate();
            }
        }
    }
}
