using System;
using System.Windows.Media;
using System.Windows.Threading;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models
{
    public static class HintMachineService
    {
        // Only in WPF 4.5
        /*
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        */

        public static bool DebugBuild { get; private set; } = false;

        public static ArchipelagoHintSession ArchipelagoSession { get; private set; } = null;
        public static string Host => ArchipelagoSession?.Host ?? string.Empty;
        public static string Slot => ArchipelagoSession?.Slot ?? string.Empty;
        public static string Password => ArchipelagoSession?.Password ?? string.Empty;

        public static GameConnectionHandler CurrentGameConnection { get; private set; } = null;

        public static int HintTokens
        {
            get { return _hintTokens; }
            set 
            {
                _hintTokens = value;
                ModelChanged?.Invoke();
            }
        }
        private static int _hintTokens = 0;

        public static event Action ModelChanged = null;

        // -----------------------------------------

        static HintMachineService()
        {
            #if DEBUG
                DebugBuild = true;
            #endif
        }

        public static void OnAppExit()
        {
            DisconnectFromGame();
            DisconnectFromArchipelago();
            Settings.SaveToFile();
        }

        public static void ConnectToArchipelago(string host, string slot, string password)
        {
            ArchipelagoSession = new ArchipelagoHintSession(host, slot, password);
            ModelChanged?.Invoke();
            if (ArchipelagoSession.IsConnected)
            {
                // If connectionn succeeded, store the fields contents for next execution and move on to MainWindow
                Settings.Host = host;
                Settings.Slot = slot;
            }
            else
            {
                string error = ArchipelagoSession.ErrorMessage;
                ArchipelagoSession = null;
                throw new ArchipelagoConnectionException(error);
            }
        }

        public static void DisconnectFromArchipelago()
        {
            if (ArchipelagoSession == null)
                return;

            ArchipelagoSession.Disconnect();
            ArchipelagoSession = null;
            ModelChanged?.Invoke();
            Logger.Info("Disconnected from Archipelago.");
        }

        public static bool ConnectToGame(IGameConnector game)
        {
            if (CurrentGameConnection != null)
            {
                Logger.Error("Attempted connecting to a game while there is another ongoing connection.");
                return false;
            }

            if(!game.DoConnect())
            {
                return false;
            }

            CurrentGameConnection = new GameConnectionHandler(game, Dispatcher.CurrentDispatcher);
            
            CurrentGameConnection.GameDisconnected += () => {
                CurrentGameConnection = null;
                ModelChanged?.Invoke();
            };

            CurrentGameConnection.HintTokensEarned += (int amount) => { HintTokens += amount; };
            ModelChanged?.Invoke();
            Logger.Info($"✔️ Successfully connected to {game.Name}. ");
            return true;
        }

        public static void DisconnectFromGame()
        {
            if (CurrentGameConnection == null)
                return;

            CurrentGameConnection.Disconnect();
            ModelChanged?.Invoke();
            Logger.Info("Disconnected from game.");
        }

        public static void SubmitChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string normalizedMessage = message.Trim().ToLower();

            if (normalizedMessage == "!charly")
            {
                int index = new Random().Next(Globals.CharlyMachineFacts.Count);
                Logger.Info(Globals.CharlyMachineFacts[index]);
            }
            else if (normalizedMessage == "!lulu")
            {
                int index = new Random().Next(Globals.LuluMachineFacts.Count);
                Logger.Info(Globals.LuluMachineFacts[index]);
            }
            else if (normalizedMessage == "!hitmachine" || normalizedMessage == "!hintmachine")
            {
                int index = new Random().Next(Globals.HitMachineFacts.Count);
                Logger.Info(Globals.HitMachineFacts[index]);
            }
            else if (normalizedMessage == "!about")
            {
                ShowAboutMessage();
            }
            else if (DebugBuild && normalizedMessage == "!gethint")
            {
                HintTokens += 1;
            }
            else
            {
                // Message was not a custom command, forward it to Archipelago server as a regular message
                ArchipelagoSession?.SendMessage(message);
            }
        }

        public static void ShowAboutMessage()
        {
            Logger.Info("--------------------------------------------------------------------\n"
                      + $"                               {Globals.ProgramName} v{Globals.ProgramVersion}\n"
                      + "           Developed with ❤️ by Dinopony & CalDrac \n"
                      + "--------------------------------------------------------------------");
        }

        public static void RedeemHintToken()
        {
            if (HintTokens <= 0)
                return;

            if (ArchipelagoSession != null)
            {
                ArchipelagoSession.PendingRandomHints += 1;
                HintTokens -= 1;
            }
        }
    }
}
