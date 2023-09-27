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
        private ArchipelagoHintSession _archipelagoSession = null;
        private IGameConnector _game = null;
        private Timer _timer = null;

        // ----------------------------------------------------------------------------------

        public MainWindow(ArchipelagoHintSession archipelagoSession)
        {
            InitializeComponent();

            _archipelagoSession = archipelagoSession;
            _archipelagoSession.Client.MessageLog.OnMessageReceived += OnArchipelagoMessageReceived;
            _archipelagoSession.OnHintsUpdate += hintsList.UpdateItems;

            labelHost.Text = _archipelagoSession.Host;

            // Setup the message log by connecting it to the global Logger
            SetupChatFilterMenus();
            Logger.OnMessageLogged = OnMessageLogged;
            OnSlotConnected();
            Logger.Info("Feeling stuck in your Archipelago world?\n" +
                        "Connect to a game and start playing to get random hints instead of eating good old Burger King.");

            PopulateGamesCombobox();

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _timer = new Timer { AutoReset = true, Interval = 100 };
            _timer.Elapsed += OnTimerTick;
            _timer.AutoReset = true;
            _timer.Enabled = true;

        }

        /// <summary>
        /// Populate game selector combobox with all currently supported game names
        /// </summary>
        protected void PopulateGamesCombobox()
        {
            Globals.Games.Sort((a, b) => a.Name.CompareTo(b.Name));
            foreach (IGameConnector connector in Globals.Games)
            {
                gameComboBox.Items.Add(connector.Name);

                if (connector.Name == Settings.Game)
                    gameComboBox.SelectedItem = gameComboBox.Items[gameComboBox.Items.Count - 1];
            }

            if (gameComboBox.SelectedItem == null)
                gameComboBox.SelectedItem = gameComboBox.Items[0];
        }

        protected void OnSlotConnected()
        {
            labelSlot.Text = _archipelagoSession.Slot;

            SetupHintsTab();

            // Setup "Reconnect as..." menu
            menuReconnect.Items.Clear();
            foreach (string playerName in _archipelagoSession.GetPlayerNames())
            {
                if (playerName == "Server")
                    continue;

                MenuItem subItem = new MenuItem { Header = playerName };

                if (playerName != _archipelagoSession.Slot)
                    subItem.Click += (s, e) => { OnReconnectAsPlayerClick(playerName); };
                else
                {
                    subItem.IsEnabled = false;
                    subItem.IsChecked = true;
                }

                menuReconnect.Items.Add(subItem);
            }

            Logger.Info("Connected to Archipelago session at " + _archipelagoSession.Host + " as " + _archipelagoSession.Slot + ".");
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

        private void OnTimerTick(object sender, ElapsedEventArgs e)
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

            if (!pollSuccessful && _game != null)
            {
                Logger.Error("Connection with " + _game.Name + " was lost.");
                DisconnectFromGame();
                return;
            }
            if (_game != null)
            {
                // Update hint quests
                foreach (HintQuest quest in _game.Quests)
                {
                    if (quest.CheckCompletion())
                    {
                        for (int i = 0; i < quest.AwardedHints ; i++)
                        {
                            _archipelagoSession.GetOneRandomHint(_game.Name);
                        }
                    }

                    Dispatcher.Invoke(() => { quest.UpdateComponents(); });
                }
            }
        }

        private void OnConnectToGameButtonClick(object sender, RoutedEventArgs e)
        {
            if (_game != null)
                return;

            // Connect to selected game
            string selectedGameName = gameComboBox.SelectedValue.ToString();
            IGameConnector game = Globals.FindGameFromName(selectedGameName);
            if (game.Connect())
            {
                _game = game;
                Title = Globals.ProgramName + " - " + _game.Name;
                labelGame.Text = _game.Name;

                // Init game quests
                foreach (HintQuest quest in _game.Quests)
                {
                    quest.InitComponents(questsGrid);
                    quest.UpdateComponents();
                }

                gameConnectGrid.Visibility = Visibility.Hidden;
                questsGrid.Visibility = Visibility.Visible;
                buttonChangeGame.Visibility = Visibility.Visible;

                // Store last selected game in settings to automatically select it on next execution
                Settings.Game = selectedGameName;

                Logger.Info("✔️ Successfully connected to " + game.Name + ". ");
            }
            else
            {
                Logger.Error("Could not connect to " + game.Name + ". " +
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

                Title = Globals.ProgramName;
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
            IGameConnector game = Globals.FindGameFromName(selectedGameName);

            textblockGameDescription.Text = game.Description;

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
            LogMessageType type = LogMessageType.RAW;
            List<MessagePart> parts = Enumerable.ToList(message.Parts);

            if (message is JoinLogMessage || message is LeaveLogMessage)
                type = LogMessageType.JOIN_LEAVE;
            else if (message is HintItemSendLogMessage)
            {
                type = LogMessageType.HINT;
                parts.RemoveAt(0); // Remove the [Hint] prefix
            }
            else if (message is ItemSendLogMessage)
            {
                if (((ItemSendLogMessage)message).Sender.Name == _archipelagoSession.Slot)
                    type = LogMessageType.ITEM_SENT;
                else if (((ItemSendLogMessage)message).Receiver.Name == _archipelagoSession.Slot)
                    type = LogMessageType.ITEM_RECEIVED;
                else
                    return;
            }
            else if (message is ChatLogMessage || message is ServerChatLogMessage)
                type = LogMessageType.CHAT;
            else if (message is CommandResultLogMessage)
                type = LogMessageType.SERVER_RESPONSE;
            else if (message is GoalLogMessage)
                type = LogMessageType.GOAL;

            string str = "";
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
                { menuDisplayJoinLeaveMessages, Settings.DisplayJoinLeaveMessages },
                { menuDisplayReceivedItems, Settings.DisplayItemReceivedMessages },
                { menuDisplaySentItems, Settings.DisplayItemSentMessages },
            };

            foreach (var kv in MENU_ITEMS)
            {
                kv.Key.IsChecked = kv.Value;
                kv.Key.Checked += OnFilterChange;
                kv.Key.Unchecked += OnFilterChange;
            }
        }

        /// <summary>
        /// A setup procedure called on init and whenever the "Hints" tab is opened by the user to
        /// update the hints view and other elements contained in this tab.
        /// </summary>
        private void SetupHintsTab()
        {
            // Calculate the available hints
            int remainingHints = _archipelagoSession.GetAvailableHintsWithHintPoints();
            int checksBeforeHint = _archipelagoSession.GetCheckCountBeforeNextHint();
            availableHintsLabel.Content = $"You have {remainingHints} remaining hints, you will get a new hint in {checksBeforeHint} checks.";

            manualHintButton.IsEnabled = (remainingHints > 0);
            hintsList.UpdateItems(_archipelagoSession.KnownHints);
        }

        private void OnFilterChange(object sender, RoutedEventArgs e)
        {
            Settings.DisplayChatMessages = menuDisplayChatMessages.IsChecked;
            Settings.DisplayFoundHintMessages = menuDisplayFoundHints.IsChecked;
            Settings.DisplayJoinLeaveMessages = menuDisplayJoinLeaveMessages.IsChecked;
            Settings.DisplayItemReceivedMessages = menuDisplayReceivedItems.IsChecked;
            Settings.DisplayItemSentMessages = menuDisplaySentItems.IsChecked;
            messageLog.UpdateMessagesVisibility();
        }

        private void OnExitMenuClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("-----------------------------------------------\n"
                      + $"{Globals.ProgramName} v{Globals.ProgramVersion}\n"
                      + "Developed with ❤️ by Dinopony & CalDrac \n"
                      + "-----------------------------------------------");
            // Force a switch to the message log tab to see the newly added message
            tabControl.SelectedIndex = 0;
        }

        private void OnTabChange(object sender, RoutedEventArgs e)
        {
            if (e.Source is TabControl && tabControl.SelectedIndex == 1)
                SetupHintsTab();
        }

        private void OnManualItemHint(string itemName)
        {
            _archipelagoSession.SendMessage("!hint " + itemName);
            // Force a switch to the message log tab to see the response to the hint request
            tabControl.SelectedIndex = 0;
        }

        private void OnManualLocationHint(string locationName)
        {
            _archipelagoSession.SendMessage("!hint_location " + locationName);
            // Force a switch to the message log tab to see the response to the hint request
            tabControl.SelectedIndex = 0;
        }

        private void OnManualHintButtonClick(object sender, RoutedEventArgs e)
        {
            ManualHintWindow window = new ManualHintWindow(_archipelagoSession);
            window.HintLocationCallback = OnManualLocationHint;
            window.HintItemCallback = OnManualItemHint;
            window.ShowDialog();
        }

        private void OnReconnectAsPlayerClick(string slotName)
        {
            string host = _archipelagoSession.Host;
            string password = _archipelagoSession.Password;

            _archipelagoSession.Disconnect();

            _archipelagoSession = new ArchipelagoHintSession(host, slotName, password);
            if (!_archipelagoSession.IsConnected)
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
