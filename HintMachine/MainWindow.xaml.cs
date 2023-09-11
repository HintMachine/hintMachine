using System.Windows;
using System.Timers;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string WINDOW_TITLE = "HintMachine";

        private readonly ArchipelagoHintSession _archipelagoSession = null;
        private IGameConnector _game = null;
        private readonly Timer _timer = null;

        public MainWindow(ArchipelagoHintSession archipelagoSession)
        {
            InitializeComponent();

            _archipelagoSession = archipelagoSession;

            // Populate game selector combobox with supported game names
            GamesList.GAMES.Sort((a, b) => a.GetDisplayName().CompareTo(b.GetDisplayName()));
            foreach (IGameConnector connector in GamesList.GAMES)
            {
                gameComboBox.Items.Add(connector.GetDisplayName());

                if (connector.GetDisplayName() == Settings.Game)
                    gameComboBox.SelectedItem = gameComboBox.Items[gameComboBox.Items.Count - 1];
            }

            _timer = new Timer { AutoReset = true, Interval = 100 };
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Log("------------ HintMachine 1.0 ------------", LogMessageType.INFO);
            Log("Feeling stuck in your Archipelago world?\n" +
                "Connect to a game and start playing to get random hints instead of eating a good old Burger King.", 
                LogMessageType.INFO);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Close the app when closing the window
            base.OnClosed(e);
            _timer.Enabled = false;
            _game.Disconnect();
            Application.Current.Shutdown();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_game == null)
                return;

            _game.Poll();

            foreach(HintQuest quest in _game.quests)
            {
                if (quest.CheckCompletion())
                {
                    Log("Congratulations on achieving the '" + quest.displayName + "' objective. " +
                        "Here's a hint for your efforts!", LogMessageType.INFO);
                    string hint = _archipelagoSession.GetOneRandomHint();
                    if (hint.Length != 0)
                        Log("❓ " + hint, LogMessageType.HINT);
                    else
                        Log("[ERROR] Couldn't fetch hint?", LogMessageType.ERROR);
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
                gameName.Content = _game.GetDisplayName();

                // Init game quests
                foreach (HintQuest quest in _game.quests)
                {
                    quest.InitComponents(questsGrid);
                    quest.UpdateComponents();
                }

                gameConnectGrid.Visibility = Visibility.Hidden;
                gameActivePanel.Visibility = Visibility.Visible;

                // Store selected game in settings file to select it first on next execution
                Settings.Game = selectedGameName;
                Settings.SaveToFile();

                Log("✔️ Successfully connected to " + game.GetDisplayName() + ". " +
                    "Complete gauges on the left panel by playing the game in order to get random hints.", LogMessageType.INFO);
            }
            else
            {
                Log("❌ [Error] Could not connect to " + game.GetDisplayName() + ". " +
                    "Please ensure it is currently running and try again.", LogMessageType.ERROR);
            }
        }
        private void OnDisconnectFromGameButtonClick(object sender, RoutedEventArgs e)
        {
            if (_game == null)
                return;

            _game = null;

            gameConnectGrid.Visibility = Visibility.Visible;
            gameActivePanel.Visibility = Visibility.Hidden;

            questsGrid.Children.Clear();
            questsGrid.RowDefinitions.Clear();

            Title = WINDOW_TITLE;

            Log("Disconnected from game.", LogMessageType.INFO);
        }

        public enum LogMessageType
        {
            RAW = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 3,
            HINT = 4,
        }

        private Color GetColorForLogMessageType(LogMessageType logMessageType)
        {
            if (logMessageType == LogMessageType.INFO)
                return Color.FromRgb(50, 50, 150);
            else if (logMessageType == LogMessageType.WARNING)
                return Color.FromRgb(128, 100, 0);
            else if (logMessageType == LogMessageType.ERROR)
                return Color.FromRgb(180, 40, 40);
            else if (logMessageType == LogMessageType.HINT)
                return Color.FromRgb(40, 180, 40);
            return Colors.Black;
        }

        public void Log(string message, LogMessageType logMessageType = LogMessageType.RAW)
        {
            Console.WriteLine(message);
            Dispatcher.Invoke(() =>
            {
                TextBlock messageBlock = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(GetColorForLogMessageType(logMessageType)),
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

    }
}
