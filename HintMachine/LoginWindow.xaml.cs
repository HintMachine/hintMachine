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

namespace HintMachine
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

            checkIfUpdateAvailableAsync();
        }

        private void OnConnectButtonClick(object sender, RoutedEventArgs e)
        {
            // Try connecting to Archipelago
            string host = InputHost.Text;
            string slot = InputSlot.Text;
            string password = InputPassword.Text;

            ArchipelagoHintSession archipelagoSession = new ArchipelagoHintSession(host, slot, password);
            if (archipelagoSession.IsConnected)
            {
                // If connectionn succeeded, store the fields contents for next execution and move on to MainWindow
                Settings.Host = host;
                Settings.Slot = slot;
                Settings.SaveToFile();

                new MainWindow(archipelagoSession).Show();
                Close();
            }
            else
            {
                MessageBox.Show($"Could not connect to Archipelago: {archipelagoSession.ErrorMessage}", "Connection error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task checkIfUpdateAvailableAsync()
        {
            
            // https://api.github.com/repos/CalDrac/hintMachine/releases

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            Console.WriteLine("");

            var json = await client.GetStringAsync("https://api.github.com/repos/CalDrac/hintMachine/releases");

            JsonArray tagsNode = JsonNode.Parse(json).AsArray();

            string lastVersion = "";
            foreach (JsonObject child in tagsNode)
            {
                if (child.ContainsKey("tag_name"))
                {
                    JsonNode tagNameNode = child["tag_name"];
                    if (!tagNameNode.ToString().Contains("rc"))
                    {
                        lastVersion = tagNameNode.ToString();
                        break;
                    }
                }
            }
            Version prog = new Version(Globals.ProgramVersion);
            Version gitVersion = new Version(lastVersion);

            if (prog.CompareTo(gitVersion) < 0) {
                MessageBox.Show("A new version is available.", "Update available", MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }
    }
}
