using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class UpdateAvailablePopup : Window
    {
        public UpdateAvailablePopup()
        {
            InitializeComponent();

            Settings.LoadFromFile();
            
        }

        private void OnGoToReleasePageButtonClick(object sender, RoutedEventArgs e)
        {
            //open github page
            ProcessStartInfo sInfo = new ProcessStartInfo("https://github.com/CalDrac/hintMachine/releases");
            Process.Start(sInfo);
            this.Close();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnCloseAndDontShowAgainButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.ShowUpdatePopUp = false;
            this.Close();
        }
    }
}
