using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;

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

            // Check that the app was launched with admin rights to be able to hook on any process
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Please launch as administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            // Fill game selector combobox with supported game names
            GamesList.GAMES.Sort((a,b) => a.GetDisplayName().CompareTo(b.GetDisplayName()));
            foreach (IGameConnector game in GamesList.GAMES)
            {
                gameComboBox.Items.Add(game.GetDisplayName());
            }

            LoadFieldsContents();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Connect to Archipelago
            ArchipelagoHintSession archipelagoSession = new ArchipelagoHintSession(archipelagoAddress.Text, slotName.Text);
            if (!archipelagoSession.isConnected)
            {
                MessageBox.Show("Could not connect to Archipelago: " + archipelagoSession.errorMessage, "Connection error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Connect to selected game
            string selectedGameName = gameComboBox.SelectedValue.ToString();
            IGameConnector game = GamesList.FindGameFromName(selectedGameName);
            if(!game.Connect())
            {
                MessageBox.Show("Could not connect to the selected game. Please ensure it is currently running.", "Error",
                                 MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // If both connections succeeded, store the fields contents for next execution and move on to MainWindow
            SaveFieldsContents();
            new MainWindow(archipelagoSession, game).Show();
            Hide();
        }

        private void SaveFieldsContents()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>() {
                { "host", archipelagoAddress.Text },
                { "slot", slotName.Text },
                { "game", gameComboBox.Items[gameComboBox.SelectedIndex].ToString() }
            };
            File.WriteAllLines("settings.cfg", dict.Select(x => x.Key + "=" + x.Value).ToArray());
        }

        private void LoadFieldsContents()
        {
            try
            {
                string[] lines = File.ReadAllLines("settings.cfg");
                foreach (var line in lines)
                {
                    int idx = line.IndexOf('=');
                    if (idx == -1)
                        continue;

                    string value = line.Substring(idx + 1);
                    if (line.StartsWith("host"))
                        archipelagoAddress.Text = value;
                    else if (line.StartsWith("slot"))
                        slotName.Text = value;
                    else if (line.StartsWith("game"))
                    {
                        foreach (var item in gameComboBox.Items)
                        {
                            if (item.ToString() == value)
                            {
                                gameComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
            catch {}
        }

        protected override void OnClosed(EventArgs e)
        {
            // Close the app when closing the window
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
