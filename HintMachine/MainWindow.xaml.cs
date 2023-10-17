using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using WMPLib;
using HintMachine.Games;
using System.Threading;
using System.Windows.Threading;


namespace HintMachine
{
    public partial class MainWindow : Window
    {
        public const int TAB_MESSAGE_LOG = 0;
        public const int TAB_HINTS = 1;

        private ArchipelagoHintSession _archipelagoSession = null;
      
        private IGameConnector _game = null;
        private readonly object _gameLock = new object();

        private readonly Thread _gameWatchingThread = null;

        private readonly WindowsMediaPlayer _soundPlayer = new WindowsMediaPlayer();

        private int _hintTokens = 0;
        private bool alreadyAwardedTokenForCurrentGame = false;

        // ----------------------------------------------------------------------------------

        public MainWindow(ArchipelagoHintSession archipelagoSession)
        {
            InitializeComponent();
            SetupChatFilterMenus();

            // Setup the message log by connecting it to the global Logger
            Logger.OnMessageLogged += (string message, LogMessageType logMessageType) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    MessageLog.AddMessage(message, logMessageType);
                }));
            };

            _archipelagoSession = archipelagoSession;
            OnArchipelagoSessionChange();

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _gameWatchingThread = new Thread(() => 
            {
                while (true)
                {
                    OnTimerTick();
                    Thread.Sleep(Globals.TickInterval);
                }
            });
            _gameWatchingThread.IsBackground = true;
            _gameWatchingThread.Start();

            // Setup the sound player that is used to play a notification sound when getting a hint
            _soundPlayer.settings.autoStart = false;
            _soundPlayer.URL = Globals.NotificationSoundPath;
            _soundPlayer.settings.volume = 30;

            Logger.Info("Feeling stuck in your Archipelago world?\n" +
                        "Connect to a game and start playing to earn hint tokens by completing quests.\n" + 
                        "You can then redeem those tokens using the dedicated button to earn a random location hint for your world.");
        }

        protected void PopulateReconnectAsMenu()
        {
            MenuReconnect.Items.Clear();
            foreach (string playerName in _archipelagoSession.GetPlayerNames())
            {
                if (playerName == "Server")
                    continue;

                MenuItem subItem = new MenuItem { Header = playerName.Replace("_", "__") };

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
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    if (TabControl.SelectedIndex == TAB_HINTS)
                        HintsView.UpdateItems(knownHints);
                }));
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

            _gameWatchingThread.Abort();
            _game?.Disconnect();

            _archipelagoSession.Disconnect();
            _archipelagoSession = null;

            Settings.SaveToFile();
        }

        private void OnTimerTick()
        {
            bool pollSuccessful = false;

            lock (_gameLock)
            {
                if (_game == null)
                    return;

                // Poll game connector, and cleanly close it if something wrong happens
                try
                {
                    pollSuccessful = _game.Poll();
                }
                catch (ProcessRamWatcherException e)
                {
                    Logger.Debug(e.Message);
                }

                if (pollSuccessful)
                {
                    int totalObtainedHintTokens = 0;

                    // Update hint quests
                    foreach (HintQuest quest in _game.Quests)
                    {
                        int obtainedHintTokens = quest.CheckAndCommitCompletion();
                        if(obtainedHintTokens > 0)
                            Logger.Debug($"Quest '{quest.Name}' completed");
                        Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => { 
                            quest.UpdateComponents(); 
                        }));
                        totalObtainedHintTokens += obtainedHintTokens;
                    }

                    if (totalObtainedHintTokens > 0)
                    {
                        if (Settings.PlaySoundOnHint)
                            _soundPlayer.controls.play();
                        if (!alreadyAwardedTokenForCurrentGame) { 
                            string hintSingularPlural = (totalObtainedHintTokens > 1) ? "hints" : "a hint";
                            _archipelagoSession.SendMessage($"I just got {hintSingularPlural} using HintMachine while playing {_game.Name}!");
                            alreadyAwardedTokenForCurrentGame = true;
                        }
                        _hintTokens += totalObtainedHintTokens;
                        UpdateHintTokensCount();
                    }
                }
            }
            
            if (!pollSuccessful)
            {
                Logger.Error($"Connection with {_game.Name} was lost.");
                DisconnectFromGame();
            }
        }

        private void UpdateHintTokensCount()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                ButtonRedeemHintToken.IsEnabled = (_hintTokens > 0);
                TextHintTokenCount.Text = $"You currently have {_hintTokens} hint tokens.";
            }));
        }

        private void OnConnectToGameButtonClick(object sender, RoutedEventArgs e)
        {
            GameSelectionWindow window = new GameSelectionWindow();
            window.OnGameConnected += OnGameConnected;
            window.ShowDialog();
        }

        public void DisconnectFromGame()
        {
            lock (_gameLock)
            {
                if (_game == null)
                    return;

                _game.Disconnect();
                _game = null;
                alreadyAwardedTokenForCurrentGame = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    GridGameConnect.Visibility = Visibility.Visible;
                    GridQuests.Visibility = Visibility.Hidden;
                    ButtonChangeGame.Visibility = Visibility.Hidden;

                    GridQuests.Children.Clear();
                    GridQuests.RowDefinitions.Clear();

                    Title = Globals.ProgramName;
                    LabelGame.Text = "-";
                }));
            }
        }

        private void OnDisconnectFromGameButtonClick(object sender, RoutedEventArgs e)
        {
            DisconnectFromGame();
            Logger.Info("Disconnected from game.");
        }

        private void OnArchipelagoDisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void OnGameConnected(IGameConnector game)
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
            Settings.LastConnectedGame = game.Name;

            Logger.Info($"✔️ Successfully connected to {game.Name}. ");
        }

        private void OnChatInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                ProcessInputMessage();
            
        }

        private void OnSendButtonClick(object sender, RoutedEventArgs e)
        {
            ProcessInputMessage();
        }

        private void ProcessInputMessage()
        {
            if (TextboxChatInput.Text == "")
                return;

            if (!ProcessCustomCommands(TextboxChatInput.Text.ToLower()))
            {
                // Message was not a custom command, forward it to Archipelago server as a regular message
                _archipelagoSession.SendMessage(TextboxChatInput.Text);
            }

            TextboxChatInput.Text = "";
        }

        private bool ProcessCustomCommands(string v)
        {
            if (v == "!charly")
            {
                int index = new Random().Next(Globals.CharlyMachineFacts.Count);
                Logger.Info(Globals.CharlyMachineFacts[index]);
                return true;
            }
            else if(v == "!lulu")
            {
                int index = new Random().Next(Globals.LuluMachineFacts.Count);
                Logger.Info(Globals.LuluMachineFacts[index]);
                return true;
            }
            else if (v == "!hitmachine" || v == "!hintmachine")
            {
                int index = new Random().Next(Globals.HitMachineFacts.Count);
                Logger.Info(Globals.HitMachineFacts[index]);
                return true;
            }
            else if(v == "!about")
            {
                OnAboutClick(null, null);
                return true;
            }

#if DEBUG   // Debug commands
            else if (v == "!gethint")
            {
                _hintTokens += 1;
                UpdateHintTokensCount();
                return true;
            }
#endif

            return false;
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
            string text;

            // Calculate the available hints using hint points
            int remainingHints = _archipelagoSession.GetAvailableHintsWithHintPoints();
            if(remainingHints == int.MaxValue)
                text = "You have infinite hints";
            else
                text = $"You have {remainingHints} remaining hints";

            int checksBeforeHint = _archipelagoSession.GetCheckCountBeforeNextHint();
            if (checksBeforeHint == int.MaxValue)
                text += ", and you cannot earn hint points.";
            else if (checksBeforeHint == 0)
                text += " because hints do not cost hint points.";
            else
                text += $", you will get a new hint in {checksBeforeHint} checks.";

            ButtonManualHint.IsEnabled = (remainingHints > 0);
            LabelAvailableHints.Content = text;

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
            ManualHintWindow window = new ManualHintWindow(_archipelagoSession)
            {
                HintLocationCallback = OnManualLocationHint,
                HintItemCallback = OnManualItemHint
            };
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

            Settings.Slot = slotName;
            Settings.SaveToFile();
            OnArchipelagoSessionChange();
        }

        private void OnRedeemHintTokenClick(object sender, RoutedEventArgs e)
        {
            if (_hintTokens <= 0)
                return;

            _archipelagoSession.PendingRandomHints += 1;
            _hintTokens -= 1;
            UpdateHintTokensCount();
        }
    }
}
