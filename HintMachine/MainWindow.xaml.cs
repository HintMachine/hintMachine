using System.Windows;
using System.Timers;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using Archipelago.MultiClient.Net;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string WINDOW_TITLE = "HintMachine";

        private ArchipelagoHintSession _archipelagoSession = null;
        private IGameConnector _game = null;
        private Timer _timer = null;

        public MainWindow(ArchipelagoHintSession archipelagoSession)
        {
            InitializeComponent();

            _archipelagoSession = archipelagoSession;
            labelHost.Content = _archipelagoSession.host;
            labelSlot.Content = _archipelagoSession.slot;

            // Populate game selector combobox with supported game names
            GamesList.GAMES.Sort((a, b) => a.GetDisplayName().CompareTo(b.GetDisplayName()));
            foreach (IGameConnector connector in GamesList.GAMES)
            {
                gameComboBox.Items.Add(connector.GetDisplayName());

                if (connector.GetDisplayName() == Settings.Game)
                    gameComboBox.SelectedItem = gameComboBox.Items[gameComboBox.Items.Count - 1];
            }

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _timer = new Timer { AutoReset = true, Interval = 100 };
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            // Setup the global Logger to populate the message log view and log a few welcome messages
            Logger.OnMessageLogged = OnMessageLogged;

            Logger.Info("------------ HintMachine 1.0 ------------");
            Logger.Info("Connected to Archipelago session at " + archipelagoSession.host + " as " + archipelagoSession.slot + ".");
            Logger.Info("Feeling stuck in your Archipelago world?\n" +
                        "Connect to a game and start playing to get random hints instead of eating good old Burger King.");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_game != null)
                _game.Disconnect();

            _timer.Enabled = false;
            _archipelagoSession = null;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_game == null)
                return;

            // Poll game connector, and cleanly close it if something wrong happens
            if(!_game.Poll())
            {
                Logger.Error("❌ [Error] Connection with " + _game.GetDisplayName() + " was lost.");
                DisconnectFromGame();
                return;
            }

            // Update hint quests
            foreach(HintQuest quest in _game.quests)
            {
                if (quest.CheckCompletion())
                {
                    Logger.Info("Congratulations on completing the '" + quest.displayName + "' objective. " +
                                "Here's a hint for your efforts!");
                    string hint = _archipelagoSession.GetOneRandomHint();
                    if (hint.Length != 0)
                        Logger.Hint("❓ " + hint);
                    else
                        Logger.Error("[ERROR] Couldn't fetch hint?");
                }

                Dispatcher.Invoke(() => { quest.UpdateComponents(); });
            }
        }

        private void OnConnectToGameButtonClick(object sender, RoutedEventArgs e)
        {
            if (_game != null)
                return;

            // Connect to selected game
            string selectedGameName = gameComboBox.SelectedValue.ToString();
            IGameConnector game = GamesList.FindGameFromName(selectedGameName);
            if (game.Connect())
            {
                _game = game;

                Title = WINDOW_TITLE + " - " + _game.GetDisplayName();
                labelGame.Content = _game.GetDisplayName();

                // Init game quests
                foreach (HintQuest quest in _game.quests)
                {
                    quest.InitComponents(questsGrid);
                    quest.UpdateComponents();
                }

                gameConnectGrid.Visibility = Visibility.Hidden;
                questsGrid.Visibility = Visibility.Visible;
                buttonChangeGame.Visibility = Visibility.Visible;

                // Store selected game in settings file to select it first on next execution
                Settings.Game = selectedGameName;
                Settings.SaveToFile();

                Logger.Info("✔️ Successfully connected to " + game.GetDisplayName() + ". ");
            }
            else
            {
                Logger.Error("❌ [Error] Could not connect to " + game.GetDisplayName() + ". " +
                             "Please ensure it is currently running and try again.");
            }
        }

        public void DisconnectFromGame()
        {
            if (_game == null)
                return;

            _game.Disconnect();
            _game = null;

            Dispatcher.Invoke(() =>
            {
                gameConnectGrid.Visibility = Visibility.Visible;
                questsGrid.Visibility = Visibility.Hidden;
                buttonChangeGame.Visibility = Visibility.Hidden;

                questsGrid.Children.Clear();
                questsGrid.RowDefinitions.Clear();

                Title = WINDOW_TITLE;
                labelGame.Content = "-";
            });
        }
        private void OnDisconnectFromGameButtonClick(object sender, RoutedEventArgs e)
        {
            DisconnectFromGame();
            Logger.Info("Disconnected from game.");
        }

        public void OnMessageLogged(string message, LogMessageType logMessageType)
        {
            Dispatcher.Invoke(() =>
            {
                TextBlock messageBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Logger.GetColorForMessageType(logMessageType)),
                    Padding = new Thickness(6, 4, 6, 4),
                    FontSize = 14,
                };

                if (messageLog.Children.Count % 2 == 1)
                    messageBlock.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                if (logMessageType == LogMessageType.ERROR)
                    messageBlock.FontWeight = FontWeights.Bold;

                // If view was already at the bottom before adding the element, auto-scroll to prevent the user from
                // having to scroll manually each time there are new messages
                bool scrollToBottom = (messageLogScrollViewer.VerticalOffset == messageLogScrollViewer.ScrollableHeight);

                messageLog.Children.Add(messageBlock);

                if (scrollToBottom)
                    messageLogScrollViewer.ScrollToBottom();
            });
        }

        private void OnArchipelagoDisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void gameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedGameName = gameComboBox.SelectedValue.ToString();
            IGameConnector game = GamesList.FindGameFromName(selectedGameName);

            textblockGameDescription.Text = game.GetDescription();

            if (textblockGameDescription.Text.Length != 0)
                textblockGameDescription.Visibility = Visibility.Visible;
            else
                textblockGameDescription.Visibility = Visibility.Collapsed;
        }
    }
}
