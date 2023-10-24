using HintMachine.Models;
using System.Diagnostics;
using System.Windows;

namespace HintMachine.Views
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
