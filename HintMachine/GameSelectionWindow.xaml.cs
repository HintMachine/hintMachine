using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace HintMachine
{
    /// <summary>
    /// Logique d'interaction pour ManualHintWindow.xaml
    /// </summary>
    public partial class GameSelectionWindow : Window
    {
        public delegate void OnGameConnectedAction(IGameConnector game);
        public event OnGameConnectedAction OnGameConnected = null;

        // ----------------------------------------------------------------------------------

        public GameSelectionWindow()
        {
            InitializeComponent();

            ListGames.ItemsSource = Globals.Games;
            ListGames.Items.SortDescriptions.Clear();
            ListGames.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            if (Settings.LastConnectedGame != "")
                ListGames.SelectedValue = Globals.FindGameFromName(Settings.LastConnectedGame);
            else
                ListGames.SelectedValue = ListGames.Items[0];
        }

        private void OnValidateButtonClick(object sender, RoutedEventArgs e)
        {
            // Connect to selected game
            IGameConnector game = ListGames.SelectedItem as IGameConnector;
            if (!game.Connect())
            {
                string message = $"Could not connect to {game.Name}.\n" +
                                  "Please ensure it is currently running and try again.";
                MessageBox.Show(message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OnGameConnected?.Invoke(game);
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnListSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            IGameConnector game = ListGames.SelectedItem as IGameConnector;
            TextGameName.Text = game.Name;
            TextGameDescription.Text = game.Description;

            BitmapImage image;
            try
            {
                image = new BitmapImage(new Uri($"./Assets/covers/{game.CoverFilename}", UriKind.Relative));
            }
            catch(FileNotFoundException)
            {
                image = new BitmapImage(new Uri($"./Assets/covers/unknown.png", UriKind.Relative));
            }
            ImageGameCover.Source = image;

            UpdateGameProperties(game);
        }

        private void UpdateGameProperties(IGameConnector game)
        {
            TextGameProperties.Text = "";
            if (game.SupportedVersions.Count > 0)
            {
                if (game.SupportedVersions.Count == 1)
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported version: ")));
                    TextGameProperties.Inlines.Add(game.SupportedVersions[0]);
                }
                else
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported versions: ")));
                    foreach (string version in game.SupportedVersions)
                    {
                        TextGameProperties.Inlines.Add(new LineBreak());
                        TextGameProperties.Inlines.Add($"    • {version}");
                    }
                }

                TextGameProperties.Inlines.Add(new LineBreak());
                TextGameProperties.Inlines.Add(new LineBreak());
            }

            if (game.SupportedEmulators.Count > 0)
            {
                if (game.SupportedEmulators.Count == 1)
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported emulator: ")));
                    TextGameProperties.Inlines.Add(game.SupportedEmulators[0]);
                }
                else
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported emulators: ")));
                    foreach (string emulator in game.SupportedEmulators)
                    {
                        TextGameProperties.Inlines.Add(new LineBreak());
                        TextGameProperties.Inlines.Add($"  • {emulator}");
                    }
                    TextGameProperties.Inlines.Add(new LineBreak());
                }
                TextGameProperties.Inlines.Add(new LineBreak());
                TextGameProperties.Inlines.Add(new LineBreak());
            }

            TextGameProperties.Inlines.Add(new Bold(new Run("Quests: ")));
            string questsString = "";
            foreach (HintQuest quest in game.Quests)
            {
                if (questsString != "")
                    questsString += ", ";
                questsString += quest.Name;
            }
            TextGameProperties.Inlines.Add(questsString);

            TextGameProperties.Inlines.Add(new LineBreak());
            TextGameProperties.Inlines.Add(new LineBreak());

            TextGameProperties.Inlines.Add(new Bold(new Run("Author: ")));
            TextGameProperties.Inlines.Add(game.Author);
        }
    }
}
