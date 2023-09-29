using System.Windows;
using System.Timers;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using WMPLib;
using HintMachine.Games;
using System.Linq;

namespace HintMachine
{
    public partial class MainWindow : Window
    {
        public const int TAB_MESSAGE_LOG = 0;
        public const int TAB_HINTS = 1;

        private ArchipelagoHintSession _archipelagoSession = null;
      
        private IGameConnector _game = null;
        private readonly object _gameLock = new object();
        private readonly Timer _pollTickTimer = null;

        private readonly WindowsMediaPlayer _soundPlayer = new WindowsMediaPlayer();

        // ----------------------------------------------------------------------------------

        public MainWindow(ArchipelagoHintSession archipelagoSession)
        {
            InitializeComponent();
            SetupChatFilterMenus();
            PopulateGamesCombobox();

            // Setup the message log by connecting it to the global Logger
            Logger.OnMessageLogged += (string message, LogMessageType logMessageType) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessageLog.AddMessage(message, logMessageType);
                });
            };

            _archipelagoSession = archipelagoSession;
            OnArchipelagoSessionChange();

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _pollTickTimer = new Timer {
                AutoReset = true,
                Interval = 100,
                Enabled = true,
            };
            _pollTickTimer.Elapsed += OnTimerTick;

            // Setup the sound player that is used to play a notification sound when getting a hint
            _soundPlayer.settings.autoStart = false;
            _soundPlayer.URL = Globals.NotificationSoundPath;
            _soundPlayer.settings.volume = 30;

            Logger.Info("Feeling stuck in your Archipelago world?\n" +
                        "Connect to a game and start playing to get random hints instead of eating good old Burger King.");
        }

        /// <summary>
        /// Populate game selector combobox with all currently supported game names
        /// </summary>
        protected void PopulateGamesCombobox()
        {
            ComboboxGame.Items.Clear();
            foreach (string gameName in Globals.Games.OrderBy(g => g.Name).Select(g => g.Name))
                ComboboxGame.Items.Add(gameName);

            ComboboxGame.SelectedValue = Settings.LastConnectedGame;
            if (ComboboxGame.SelectedItem == null)
                ComboboxGame.SelectedItem = ComboboxGame.Items[0];
        }

        protected void PopulateReconnectAsMenu()
        {
            MenuReconnect.Items.Clear();
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

                MenuReconnect.Items.Add(subItem);
            }
        }

        protected void OnArchipelagoSessionChange()
        {
            _archipelagoSession.OnHintsUpdate += (List<HintDetails> knownHints) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (TabControl.SelectedIndex == TAB_HINTS)
                        HintsView.UpdateItems(knownHints);
                });
            };
            
            LabelHost.Text = _archipelagoSession.Host;
            LabelSlot.Text = _archipelagoSession.Slot;
            PopulateReconnectAsMenu();

            // If the tab currently being open is the hints tab, refresh the hints view and the available hints count
            if (TabControl.SelectedIndex == TAB_HINTS)
                SetupHintsTab();

            Logger.Info("Connected to Archipelago session at " + _archipelagoSession.Host + " as " + _archipelagoSession.Slot + ".");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            lock (_gameLock)
            {
                _game?.Disconnect();
                _pollTickTimer.Enabled = false;
            }

            _archipelagoSession.Disconnect();
            _archipelagoSession = null;

            Settings.SaveToFile();
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            lock (_gameLock)
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
                            if (Settings.PlaySoundOnHint)
                                _soundPlayer.controls.play();

                            for (int i = 0; i < quest.AwardedHints; i++)
                                _archipelagoSession.GetOneRandomHint(_game.Name);
                        }

                        Dispatcher.Invoke(() => { quest.UpdateComponents(); });
                    }
                }
            }
        }

        private void OnConnectToGameButtonClick(object sender, RoutedEventArgs e)
        {
            lock (_gameLock)
            {
                if (_game != null)
                    return;

                // Connect to selected game
                string selectedGameName = ComboboxGame.SelectedValue.ToString();
                IGameConnector game = Globals.FindGameFromName(selectedGameName);
                if (game.Connect())
                {
                    _game = game;
                    Title = Globals.ProgramName + " - " + _game.Name;
                    LabelGame.Text = _game.Name;

                    // Init game quests
                    foreach (HintQuest quest in _game.Quests)
                    {
                        quest.InitComponents(GridQuests);
                        quest.UpdateComponents();
                    }

                    GridGameConnect.Visibility = Visibility.Hidden;
                    GridQuests.Visibility = Visibility.Visible;
                    ButtonChangeGame.Visibility = Visibility.Visible;

                    // Store last selected game in settings to automatically select it on next execution
                    Settings.LastConnectedGame = selectedGameName;

                    Logger.Info("✔️ Successfully connected to " + game.Name + ". ");
                }
                else
                {
                    Logger.Error("Could not connect to " + game.Name + ". " +
                                 "Please ensure it is currently running and try again.");
                }
            }
        }

        public void DisconnectFromGame()
        {
            lock (_gameLock)
            {
                if (_game == null)
                    return;

                _game.Disconnect();
                _game = null;

                Dispatcher.Invoke(() =>
                {
                    GridGameConnect.Visibility = Visibility.Visible;
                    GridQuests.Visibility = Visibility.Hidden;
                    ButtonChangeGame.Visibility = Visibility.Hidden;

                    GridQuests.Children.Clear();
                    GridQuests.RowDefinitions.Clear();

                    Title = Globals.ProgramName;
                    LabelGame.Text = "-";
                });
            }
        }

        private void OnDisconnectFromGameButtonClick(object sender, RoutedEventArgs e)
        {
            DisconnectFromGame();
            Logger.Info("Disconnected from game.");
        }

        public void OnMessageLogged()
        {

        }

        private void OnArchipelagoDisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void OnSelectedGameConnectorChange(object sender, SelectionChangedEventArgs e)
        {
            string selectedGameName = ComboboxGame.SelectedValue.ToString();
            IGameConnector game = Globals.FindGameFromName(selectedGameName);

            TextblockGameDescription.Text = $"{game.Description}\n\n{game.SupportedVersions}";

            if (TextblockGameDescription.Text.Length != 0)
                TextblockGameDescription.Visibility = Visibility.Visible;
            else
                TextblockGameDescription.Visibility = Visibility.Collapsed;
        }

        private void SendMessageToArchipelago()
        {
            if (TextboxChatInput.Text == "")
                return;

            _archipelagoSession.SendMessage(TextboxChatInput.Text);
            TextboxChatInput.Text = "";
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

        private void SetupChatFilterMenus()
        {
            Dictionary<MenuItem, bool> MENU_ITEMS = new Dictionary<MenuItem, bool>()
            {
                { MenuDisplayChatMessages, Settings.DisplayChatMessages },
                { MenuDisplayFoundHints, Settings.DisplayFoundHintMessages },
                { MenuDisplayJoinLeaveMessages, Settings.DisplayJoinLeaveMessages },
                { MenuDisplayReceivedItems, Settings.DisplayItemReceivedMessages },
                { MenuDisplaySentItems, Settings.DisplayItemSentMessages },
                { MenuSoundNotification, Settings.PlaySoundOnHint },
            };

            foreach (var kv in MENU_ITEMS)
            {
                kv.Key.IsChecked = kv.Value;
                kv.Key.Checked += OnSettingChange;
                kv.Key.Unchecked += OnSettingChange;
            }
        }

        /// <summary>
        /// A setup procedure called on init and whenever the "Hints" tab is opened by the user to
        /// update the hints view and other elements contained in this tab.
        /// </summary>
        private void SetupHintsTab()
        {
            // Calculate the available hints using hint points
            int remainingHints = _archipelagoSession.GetAvailableHintsWithHintPoints();
            int checksBeforeHint = _archipelagoSession.GetCheckCountBeforeNextHint();
            ButtonManualHint.IsEnabled = (remainingHints > 0);
            LabelAvailableHints.Content = $"You have {remainingHints} remaining hints, you will get a new hint in {checksBeforeHint} checks.";

            // Update the hints list view
            HintsView.UpdateItems(_archipelagoSession.KnownHints);
        }

        private void OnSettingChange(object sender, RoutedEventArgs e)
        {
            Settings.DisplayChatMessages = MenuDisplayChatMessages.IsChecked;
            Settings.DisplayFoundHintMessages = MenuDisplayFoundHints.IsChecked;
            Settings.DisplayJoinLeaveMessages = MenuDisplayJoinLeaveMessages.IsChecked;
            Settings.DisplayItemReceivedMessages = MenuDisplayReceivedItems.IsChecked;
            Settings.DisplayItemSentMessages = MenuDisplaySentItems.IsChecked;
            Settings.PlaySoundOnHint = MenuSoundNotification.IsChecked;
            MessageLog.UpdateMessagesVisibility();
        }

        private void OnExitMenuClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("--------------------------------------------------------------------\n"
                      + $"                               {Globals.ProgramName} v{Globals.ProgramVersion}\n"
                      + "           Developed with ❤️ by Dinopony & CalDrac \n"
                      + "--------------------------------------------------------------------");
            TabControl.SelectedIndex = TAB_MESSAGE_LOG;
        }

        private void OnManualItemHint(string itemName)
        {
            _archipelagoSession.SendMessage("!hint " + itemName);
            TabControl.SelectedIndex = TAB_MESSAGE_LOG;
        }

        private void OnManualLocationHint(string locationName)
        {
            _archipelagoSession.SendMessage("!hint_location " + locationName);
            TabControl.SelectedIndex = TAB_MESSAGE_LOG;
        }
        private void OnTabChange(object sender, RoutedEventArgs e)
        {
            if (e.Source is TabControl && TabControl.SelectedIndex == TAB_HINTS)
                SetupHintsTab();
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

            OnArchipelagoSessionChange();
        }
    }
}
