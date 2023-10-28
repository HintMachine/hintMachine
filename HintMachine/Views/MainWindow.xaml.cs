using HintMachine.Helpers;
using HintMachine.Models;
using HintMachine.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace HintMachine.Views
{
    public partial class MainWindow : Window
    {
        public const int TAB_MESSAGE_LOG = 0;
        public const int TAB_HINTS = 1;

        // ----------------------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();
            SetupChatFilterMenus();

            Title = $"{Globals.ProgramName} {Globals.ProgramVersion}";

            // Setup the message log by connecting it to the global Logger
            Logger.OnMessageLogged += (string message, LogMessageType logMessageType) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    MessageLog.AddMessage(message, logMessageType);
                }));
            };

            HintMachineService.GameChanged += OnGameChanged;
            OnArchipelagoSessionChange(); 

            Logger.Info("Feeling stuck in your Archipelago world?\n" +
                        "Connect to a game and start playing to earn hint tokens by completing quests.\n" + 
                        "You can then redeem those tokens using the dedicated button to earn a random location hint for your world.");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected void PopulateReconnectAsMenu()
        {
            MenuReconnect.Items.Clear();
            foreach (string playerName in HintMachineService.ArchipelagoSession.GetPlayerNames())
            {
                MenuItem subItem = new MenuItem { Header = playerName.Replace("_", "__") };

                if (playerName != HintMachineService.ArchipelagoSession.Slot)
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
            // TODO: Problematic in case of window being closed, a clean data binding would probably be more efficient
            HintMachineService.ArchipelagoSession.OnHintsUpdate += (List<HintDetails> knownHints) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    if (TabControl.SelectedIndex == TAB_HINTS)
                        HintsView.UpdateItems(knownHints);
                }));
            };

            PopulateReconnectAsMenu();

            // If the tab currently being open is the hints tab, refresh the hints view and the available hints count
            if (TabControl.SelectedIndex == TAB_HINTS)
                SetupHintsTab();

            Logger.Info($"Connected to Archipelago session at {HintMachineService.Host} as {HintMachineService.Slot}.");
        }

        private void OnGameChanged()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => 
            {
                var game = HintMachineService.CurrentGameConnection?.Game;
                if (game != null)
                {
                    // Init game quest widgets
                    foreach (HintQuest quest in game.Quests)
                        quest.InitComponents(StackPanelQuests);

                    TextCurrentGame.Visibility = Visibility.Visible;
                    ButtonDisconnectFromGame.Visibility = Visibility.Visible;
                }
                else
                {
                    // Clear game quest widgets
                    StackPanelQuests.Children.Clear();

                    TextCurrentGame.Visibility = Visibility.Hidden;
                    ButtonDisconnectFromGame.Visibility = Visibility.Hidden;
                }
            }));
        }

        private void OnConnectToGameButtonClick(object sender, RoutedEventArgs e)
        {
            new GameSelectionWindow().ShowDialog();
        }

        private void OnDisconnectFromGameButtonClick(object sender, RoutedEventArgs e)
        {
            HintMachineService.DisconnectFromGame();
        }

        private void OnArchipelagoDisconnectButtonClick(object sender, RoutedEventArgs e)
        {
            HintMachineService.DisconnectFromArchipelago();
            new LoginWindow().Show();
            Close();
        }

        private void SubmitChatMessage()
        {
            HintMachineService.SubmitChatMessage(TextboxChatInput.Text);
            TextboxChatInput.Text = "";
        }

        private void OnChatInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SubmitChatMessage();
        }

        private void OnSendButtonClick(object sender, RoutedEventArgs e) => SubmitChatMessage();

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
                { MenuShowUpdatePopup, Settings.ShowUpdatePopUp},
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
            int remainingHints = HintMachineService.ArchipelagoSession.GetAvailableHintsWithHintPoints();
            if(remainingHints == int.MaxValue)
                text = "You have infinite hints";
            else
                text = $"You have {remainingHints} remaining hints";

            int checksBeforeHint = HintMachineService.ArchipelagoSession.GetCheckCountBeforeNextHint();
            if (checksBeforeHint == int.MaxValue)
                text += ", and you cannot earn hint points.";
            else if (checksBeforeHint == 0)
                text += " because hints do not cost hint points.";
            else
                text += $", you will get a new hint in {checksBeforeHint} checks.";

            ButtonManualHint.IsEnabled = (remainingHints > 0);
            LabelAvailableHints.Content = text;

            // Update the hints list view
            HintsView.UpdateItems(HintMachineService.ArchipelagoSession.KnownHints);
        }

        private void OnSettingChange(object sender, RoutedEventArgs e)
        {
            Settings.DisplayChatMessages = MenuDisplayChatMessages.IsChecked;
            Settings.DisplayFoundHintMessages = MenuDisplayFoundHints.IsChecked;
            Settings.DisplayJoinLeaveMessages = MenuDisplayJoinLeaveMessages.IsChecked;
            Settings.DisplayItemReceivedMessages = MenuDisplayReceivedItems.IsChecked;
            Settings.DisplayItemSentMessages = MenuDisplaySentItems.IsChecked;
            Settings.PlaySoundOnHint = MenuSoundNotification.IsChecked;
            Settings.ShowUpdatePopUp = MenuShowUpdatePopup.IsChecked;
            MessageLog.UpdateMessagesVisibility();
        }

        private void OnExitMenuClick(object sender, RoutedEventArgs e) => Close();

        private void OnManualItemHint(string itemName)
        {
            HintMachineService.SubmitChatMessage($"!hint {itemName}");
            TabControl.SelectedIndex = TAB_MESSAGE_LOG;
        }

        private void OnManualLocationHint(string locationName)
        {
            HintMachineService.SubmitChatMessage($"!hint_location {locationName}");
            TabControl.SelectedIndex = TAB_MESSAGE_LOG;
        }
        private void OnTabChange(object sender, RoutedEventArgs e)
        {
            if (e.Source is TabControl && TabControl.SelectedIndex == TAB_HINTS)
                SetupHintsTab();
        }

        private void OnManualHintButtonClick(object sender, RoutedEventArgs e)
        {
            ManualHintWindow window = new ManualHintWindow()
            {
                HintLocationCallback = OnManualLocationHint,
                HintItemCallback = OnManualItemHint
            };
            window.ShowDialog();
        }

        private void OnReconnectAsPlayerClick(string slotName)
        {
            string host = HintMachineService.Host;
            string password = HintMachineService.Password;

            try
            {
                HintMachineService.DisconnectFromArchipelago();
                HintMachineService.ConnectToArchipelago(host, slotName, password);
                OnArchipelagoSessionChange(); // TODO: Data binding!
            }
            catch(ArchipelagoConnectionException)
            {
                MessageBox.Show("Could not reconnect to Archipelago server.", "Connection error",
                     MessageBoxButton.OK, MessageBoxImage.Error);
                new LoginWindow().Show();
                Close();
                return;
            }
        }

        private void OnAboutClick(object sender, RoutedEventArgs e) => HintMachineService.ShowAboutMessage();

        private void OnRedeemHintTokenClick(object sender, RoutedEventArgs e) => HintMachineService.RedeemHintToken();
    }
}
