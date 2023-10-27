using System;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using Archipelago.MultiClient.Net;
using HintMachine.Models.GenericConnectors;
using Newtonsoft.Json.Linq;

namespace HintMachine.Models
{
    public class GameConnectionHandler
    {
        public IGameConnector Game { get; private set; }

        public event Action GameDisconnected;

        public event Action<int> HintTokensEarned;

        private readonly Thread _gameWatchingThread = null;
        private bool _terminateThread = false;
        private readonly object _gameLock = new object();
        private readonly Dispatcher _dispatcher = null;
        private MediaPlayer _soundPlayer = null;
        private static bool _alreadyAwardedTokenForCurrentGame = false;

        // ------------------------------------

        public GameConnectionHandler(IGameConnector game, Dispatcher dispatcher)
        {
            Game = game;
            _dispatcher = dispatcher;

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _gameWatchingThread = new Thread(() => {
                while (!_terminateThread)
                {
                    OnTimerTick();
                    Thread.Sleep(Globals.TickInterval);
                }
            });
            _gameWatchingThread.IsBackground = true;
            _gameWatchingThread.Start();

            // Store last selected game in settings to automatically select it on next execution
            Settings.LastConnectedGame = game.Name;
        }

        ~GameConnectionHandler()
        {
            _terminateThread = true;
            _gameWatchingThread.Join();
        }

        private void OnTimerTick()
        {
            bool pollSuccessful = false;

            lock (_gameLock)
            {
                if (Game == null)
                    return;

                // Poll game connector, and cleanly close it if something wrong happens
                try
                {
                    pollSuccessful = Game.DoPoll();
                }
                catch (ProcessRamWatcherException e)
                {
                    Logger.Debug(e.Message);
                }

                if (pollSuccessful)
                {
                    int totalObtainedHintTokens = 0;

                    // Update hint quests
                    foreach (HintQuest quest in Game.Quests)
                    {
                        int obtainedHintTokens = quest.CheckAndCommitCompletion();
                        if (obtainedHintTokens > 0)
                            Logger.Debug($"Quest '{quest.Name}' completed");

                        _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => {
                            quest.UpdateComponents();
                        }));
                        totalObtainedHintTokens += obtainedHintTokens;
                    }

                    if (totalObtainedHintTokens > 0)
                    {
                        if (Settings.PlaySoundOnHint)
                        {
                            if (_soundPlayer == null)
                            {
                                _soundPlayer = new MediaPlayer();
                                _soundPlayer.Open(new Uri(Globals.NotificationSoundPath));
                                _soundPlayer.Volume = 0.3;
                            }
                            else
                            {
                                _soundPlayer.Stop();
                            }

                            _soundPlayer.Play();
                        }

                        if (!_alreadyAwardedTokenForCurrentGame)
                            HintMachineService.ArchipelagoSession?.SendMessage($"I just got a hint using HintMachine while playing {Game.Name}!");
                        _alreadyAwardedTokenForCurrentGame = true;

                        HintTokensEarned?.Invoke(totalObtainedHintTokens);
                    }
                }
            }

            if (!pollSuccessful)
            {
                Logger.Error($"Connection with {Game.Name} was lost.");
                Disconnect();
            }
        }
        
        public void Disconnect()
        {
            lock (_gameLock)
            {
                if (Game == null)
                    return;

                Game.Disconnect();
                Game = null;

                GameDisconnected?.Invoke();
            }
        }
    }
}
