using System.Windows;
using System.Timers;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using System.Linq;
using System.Collections.Generic;

namespace HintMachine
{
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
            _archipelagoSession.SetupOnMessageReceivedEvent(OnArchipelagoMessageReceived);
            _archipelagoSession.HintsView = hintsList;

            labelHost.Text = _archipelagoSession.host;

            // Populate game selector combobox with supported game names
            GamesList.GAMES.Sort((a, b) => a.GetDisplayName().CompareTo(b.GetDisplayName()));
            foreach (IGameConnector connector in GamesList.GAMES)
            {
                gameComboBox.Items.Add(connector.GetDisplayName());

                if (connector.GetDisplayName() == Settings.Game)
                    gameComboBox.SelectedItem = gameComboBox.Items[gameComboBox.Items.Count - 1];
            }

            if (gameComboBox.SelectedItem == null)
                gameComboBox.SelectedItem = gameComboBox.Items[0];

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _timer = new Timer { AutoReset = true, Interval = 100 };
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            // Setup the message log by connecting it to the global Logger
            SetupChatFilterMenus();
            Logger.OnMessageLogged = OnMessageLogged;

            OnSlotConnected();

            Logger.Info("Feeling stuck in your Archipelago world?\n" +
                        "Connect to a game and start playing to get random hints instead of eating good old Burger King.");
        }

        protected void OnSlotConnected()
        {
            labelSlot.Text = _archipelagoSession.slot;

            hintsList.UpdateItems(_archipelagoSession.KnownHints);

            // Setup "Reconnect as..." menu
            menuReconnect.Items.Clear();
            foreach (string playerName in _archipelagoSession.GetPlayerNames())
            {
                if (playerName == "Server")
                    continue;

                MenuItem subItem = new MenuItem { Header = playerName };

                if (playerName != _archipelagoSession.slot)
                    subItem.Click += (s, e) => { OnReconnectAsPlayerClick(playerName); };
                else
                {
                    subItem.IsEnabled = false;
                    subItem.IsChecked = true;
                }

                menuReconnect.Items.Add(subItem);
            }

            Logger.Info("Connected to Archipelago session at " + _archipelagoSession.host + " as " + _archipelagoSession.slot + ".");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_game != null)
                _game.Disconnect();

            _timer.Enabled = false;

            _archipelagoSession.Disconnect();
            _archipelagoSession = null;

            Settings.SaveToFile();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_game == null)
                return;

            // Poll game connector, and cleanly close it if something wrong happens
            bool pollSuccessful = false;
            try
            {
                pollSuccessful = _game.Poll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            if (!pollSuccessful)
            {
                Logger.Error("❌ [Error] Connection with " + _game.GetDisplayName() + " was lost.");
                DisconnectFromGame();
                return;
            }

            // Update hint quests
            foreach (HintQuest quest in _game.quests)
            {
                if (quest.CheckCompletion())
                {
                    for (int i = 0; i < quest.numberOfHintsGiven; i++)
                    {
                        _archipelagoSession.GetOneRandomHint(_game.GetDisplayName());
                    }
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
                labelGame.Text = _game.GetDisplayName();

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
                labelGame.Text = "-";
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
                messageLog.AddMessage(message, logMessageType);
            });
        }

        private void OnArchipelagoDisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void OnSelectedGameConnectorChange(object sender, SelectionChangedEventArgs e)
        {
            string selectedGameName = gameComboBox.SelectedValue.ToString();
            IGameConnector game = GamesList.FindGameFromName(selectedGameName);

            textblockGameDescription.Text = game.GetDescription();

            if (textblockGameDescription.Text.Length != 0)
                textblockGameDescription.Visibility = Visibility.Visible;
            else
                textblockGameDescription.Visibility = Visibility.Collapsed;
        }

        private void SendMessageToArchipelago()
        {
            if (inputChat.Text == "")
                return;

            _archipelagoSession.SendMessage(inputChat.Text);
            inputChat.Text = "";
        }

        private void OnChatInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SendMessageToArchipelago();
        }
        private void OnSendButtonClick(object sender, RoutedEventArgs e)
        {
            SendMessageToArchipelago();
        }

        private void OnArchipelagoMessageReceived(LogMessage message)
        {
            string str = "";
            LogMessageType type = LogMessageType.RAW;
            List<MessagePart> parts = Enumerable.ToList(message.Parts);

            if (message is JoinLogMessage || message is LeaveLogMessage)
            {
                if(!Settings.DisplayJoinLeaveMessages)
                    return; 
            }
            else if (message is HintItemSendLogMessage)
            {
                if (!Settings.DisplayFoundHintMessages && message.ToString().EndsWith("(found)"))
                    return;

                str += "❓ ";
                type = LogMessageType.HINT;
                parts.RemoveAt(0); // Remove the [Hint] prefix
            }
            else if (message is ItemSendLogMessage)
            {
                if (!Settings.DisplayItemNotificationMessages)
                    return;
            }
            else if (message is ChatLogMessage || message is ServerChatLogMessage)
            {
                if (!Settings.DisplayChatMessages)
                    return;
                str += "💬 ";
            }
            else if (message is CommandResultLogMessage)
                str += "  > ";
            else if (message is GoalLogMessage)
                str += "👑 ";

            foreach (var part in parts)
                str += part.Text;

            Logger.Log(str, type);
        }

        private void SetupChatFilterMenus()
        {
            Dictionary<MenuItem, bool> MENU_ITEMS = new Dictionary<MenuItem, bool>()
            {
                { menuDisplayChatMessages, Settings.DisplayChatMessages },
                { menuDisplayFoundHints, Settings.DisplayFoundHintMessages },
                { menuDisplayItemNotifications, Settings.DisplayItemNotificationMessages },
                { menuDisplayJoinLeaveMessages, Settings.DisplayJoinLeaveMessages },
            };

            foreach(var kv in MENU_ITEMS)
            {
                kv.Key.IsChecked = kv.Value;
                kv.Key.Checked += OnFilterChange;
                kv.Key.Unchecked += OnFilterChange;
            }
        }

        private void OnFilterChange(object sender, RoutedEventArgs e)
        {
            Settings.DisplayChatMessages = menuDisplayChatMessages.IsChecked;
            Settings.DisplayFoundHintMessages = menuDisplayFoundHints.IsChecked;
            Settings.DisplayItemNotificationMessages = menuDisplayItemNotifications.IsChecked;
            Settings.DisplayJoinLeaveMessages = menuDisplayJoinLeaveMessages.IsChecked;
        }

        private void OnExitMenuClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("-----------------------------------------------\n"
                      + "HintMachine v1.0\n"
                      + "Developed with ❤️ by Dinopony & Boffbad\n"
                      + "-----------------------------------------------");
            tabControl.SelectedIndex = 0;
        }

        private void SetupHintsTab()
        {
            // Calculate the available hints
            availableHintsLabel.Content = "You have " + _archipelagoSession.GetAvailableHintsWithHintPoints()
                                        + " remaining hints, you will get a new hint in " +
                                        _archipelagoSession.GetCheckCountBeforeNextHint() + " checks.";
        }

        private void OnTabChange(object sender, RoutedEventArgs e)
        {
            if (e.Source is TabControl && tabControl.SelectedIndex == 1)
                SetupHintsTab();
        }

        private void OnManualItemHint(string itemName)
        {
            _archipelagoSession.SendMessage("!hint " + itemName);
            tabControl.SelectedIndex = 0;
        }

        private void OnManualLocationHint(string locationName)
        {
            _archipelagoSession.SendMessage("!hint_location " + locationName);
            tabControl.SelectedIndex = 0;
        }

        private void OnManualHintButtonClick(object sender, RoutedEventArgs e)
        {
            ManualHintWindow window = new ManualHintWindow(_archipelagoSession);
            window.LocationHintCallback = OnManualLocationHint;
            window.ItemHintCallback = OnManualItemHint;
            window.ShowDialog();
        }

        private void OnReconnectAsPlayerClick(string slotName)
        {
            string host = _archipelagoSession.host;
            string password = _archipelagoSession.password;
            
            _archipelagoSession.Disconnect();

            _archipelagoSession = new ArchipelagoHintSession(host, slotName, password);
            if (!_archipelagoSession.isConnected)
            {
                MessageBox.Show("Could not reconnect to Archipelago server.", "Connection error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                new LoginWindow().Show();
                Close();
                return;
            }

            OnSlotConnected();
        }
    }
}
