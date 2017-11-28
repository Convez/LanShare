﻿using System;
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
        private TrayIconWindow trayIconWindow;

        public TransfersWindow()
        {
            InitializeComponent();
        }

        public TransfersWindow(TrayIconWindow trayIconWindow)
        {
            this.trayIconWindow = trayIconWindow;
            InitializeComponent();
        }

        public void AddTransfer(object sender, Transfer t)
        {

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
        }
    }
}
