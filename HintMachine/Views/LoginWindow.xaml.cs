using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using HintMachine.Models;
using HintMachine.Services;
using Newtonsoft.Json.Linq;

namespace HintMachine.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            Settings.LoadFromFile();
            InputHost.Text = Settings.Host;
            InputSlot.Text = Settings.Slot;

            if (Settings.ShowUpdatePopUp) {
                _ = CheckIfUpdateAvailableAsync();
            }
        }

        private void OnConnectButtonClick(object sender, RoutedEventArgs e)
        {
            // Try connecting to Archipelago
            string host = InputHost.Text;
            string slot = InputSlot.Text;
            string password = InputPassword.Text;

            try
            {
                HintMachineService.ConnectToArchipelago(host, slot, password);
                new MainWindow().Show();
                Close();
            }
            catch(ArchipelagoConnectionException ex)
            {
                MessageBox.Show($"Could not connect to Archipelago: {ex.Message}",
                                "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CheckIfUpdateAvailableAsync()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            try
            {
                var response = await client.GetStringAsync("https://api.github.com/repos/CalDrac/hintMachine/releases");
                var responseJson = JArray.Parse(response);

                string lastVersion = "";
                foreach (var release in responseJson)
                {
                    if (release["prerelease"].ToString() == "False")
                    {
                        // The first non-prerelease is the latest release
                        lastVersion = release["tag_name"].ToString();
                        break;
                    }
                }
                Version currentVersion = new Version(Globals.ProgramVersion);
                Version latestVersion = new Version(lastVersion);
                Console.WriteLine($"Latest version is {latestVersion}");

                if (currentVersion.CompareTo(latestVersion) < 0)
                {
                    new UpdateAvailablePopup().Show();
                }
            }
            catch
            {
                Console.WriteLine("Couldn't fetch latest version from GitHub API");
            }
        }
    }
}
