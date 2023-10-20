using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HintMachine.GenericConnectors;

namespace HintMachine
{
    /// <summary>
    /// Logique d'interaction pour ManualHintWindow.xaml
    /// </summary>
    public partial class GameSelectionWindow : Window
    {
        public delegate void OnGameConnectedAction(IGameConnector game);
        public event OnGameConnectedAction OnGameConnected = null;

        private const string EMPTY_SEARCH_BAR_TEXT = "🔎 Search for games...";
        private bool _emptySearchBar = true;
        private IGameConnector _selectedGame = null;

        // ----------------------------------------------------------------------------------

        public GameSelectionWindow()
        {
            InitializeComponent();

            ListGames.ItemsSource = Globals.Games;
            ListGames.Items.SortDescriptions.Clear();
            ListGames.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            ListGames.Items.Filter = null;

            _selectedGame = Globals.FindGameFromName(Settings.LastConnectedGame);
            if(_selectedGame == null)
                _selectedGame = ListGames.Items[0] as IGameConnector;
            ListGames.SelectedValue = _selectedGame;

            UpdateSearchBarFocus(false);
        }

        private void OnListSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (ListGames.SelectedItem == null)
                return;

            IGameConnector game = ListGames.SelectedItem as IGameConnector;
            
            ImageGameCover.Visibility = Visibility.Visible;
            TextGameName.Text = game.Name;
            TextGameDescription.Text = game.Description;

            BitmapImage image;
            try
            {
                image = new BitmapImage(new Uri($"./Assets/covers/{game.CoverFilename}", UriKind.Relative));
            }
            catch (FileNotFoundException)
            {
                image = new BitmapImage(new Uri($"./Assets/covers/unknown.png", UriKind.Relative));
            }
            ImageGameCover.Source = image;

            _selectedGame = game;
            UpdateSelectedGameProperties();
        }

        private void OnValidateButtonClick(object sender, RoutedEventArgs e)
        {
            ValidateGameSelection();
        }

        private void ValidateGameSelection() {

            // Connect to selected game
            if (!_selectedGame.DoConnect())
            {
                string message = $"Could not connect to {_selectedGame.Name}.\n" +
                                  "Please ensure it is currently running and try again.";
                MessageBox.Show(message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OnGameConnected?.Invoke(_selectedGame);
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnSearchTextBoxUpdate(object sender, TextChangedEventArgs e)
        {
            if (ListGames == null)
                return;

            if (_emptySearchBar && TextBoxSearch.Text == EMPTY_SEARCH_BAR_TEXT)
                return;

            _emptySearchBar = (TextBoxSearch.Text == string.Empty);

            IGameConnector selectedGame = ListGames.SelectedItem as IGameConnector;

            string searchString = TextBoxSearch.Text.Trim().ToLower();
            if (searchString != string.Empty)
            {
                string[] searchWords = searchString.Split(' ');
                ListGames.Items.Filter = obj =>
                {
                    IGameConnector game = obj as IGameConnector;
                    foreach (string word in searchWords)
                    {
                        if (game.Name.ToLower().Contains(word))
                            continue;
                        if (game.Platform.ToLower().Contains(word))
                            continue;
                        return false;
                    }
                    return true;
                };
            }
            else
            {
                ListGames.Items.Filter = null;
            }

            ListGames.SelectedItem = selectedGame;
        }

        private void UpdateSearchBarFocus(bool focused)
        {
            if(_emptySearchBar)
            {
                if(focused)
                {
                    TextBoxSearch.Text = "";
                    TextBoxSearch.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    TextBoxSearch.Text = EMPTY_SEARCH_BAR_TEXT;
                    TextBoxSearch.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void OnSearchTextboxFocus(object sender, RoutedEventArgs e) => UpdateSearchBarFocus(true);

        private void OnSearchTextboxUnfocus(object sender, RoutedEventArgs e) => UpdateSearchBarFocus(false);

        private void OnClearSearchButtonClick(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = "";
            UpdateSearchBarFocus(false);
        }

        private void UpdateSelectedGameProperties()
        {
            if (_selectedGame == null)
                return;

            TextGameProperties.Text = "";

            // Supported versions
            if (_selectedGame.SupportedVersions.Count > 0)
            {
                if (_selectedGame.SupportedVersions.Count == 1)
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported version: ")));
                    TextGameProperties.Inlines.Add(_selectedGame.SupportedVersions[0]);
                }
                else
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported versions: ")));
                    foreach (string version in _selectedGame.SupportedVersions)
                    {
                        TextGameProperties.Inlines.Add(new LineBreak());
                        TextGameProperties.Inlines.Add($"    • {version}");
                    }
                }

                TextGameProperties.Inlines.Add(new LineBreak());
                TextGameProperties.Inlines.Add(new LineBreak());
            }

            // Supported emulators
            if (_selectedGame.SupportedEmulators.Count > 0)
            {
                if (_selectedGame.SupportedEmulators.Count == 1)
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported emulator: ")));
                    TextGameProperties.Inlines.Add(_selectedGame.SupportedEmulators[0]);
                }
                else
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported emulators: ")));
                    foreach (string emulator in _selectedGame.SupportedEmulators)
                    {
                        TextGameProperties.Inlines.Add(new LineBreak());
                        TextGameProperties.Inlines.Add($"  • {emulator}");
                    }
                }
                TextGameProperties.Inlines.Add(new LineBreak());
                TextGameProperties.Inlines.Add(new LineBreak());
            }

            // Quests
            TextGameProperties.Inlines.Add(new Bold(new Run("Quests: ")));

            string questsString = "";
            foreach (HintQuest quest in _selectedGame.Quests)
            {
                if (questsString != "")
                    questsString += ", ";
                questsString += quest.Name;
            }
            TextGameProperties.Inlines.Add(questsString);

            TextGameProperties.Inlines.Add(new LineBreak());
            TextGameProperties.Inlines.Add(new LineBreak());

            // Author
            TextGameProperties.Inlines.Add(new Bold(new Run("Author: ")));
            TextGameProperties.Inlines.Add(_selectedGame.Author);
        }

        private void ListGames_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ListGames.SelectedItem != null) {
                ValidateGameSelection();
            }
        }
    }
}
