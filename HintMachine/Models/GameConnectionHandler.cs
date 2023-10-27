using System;
using System.Threading;
using System.Windows.Threading;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models
{
    public class GameConnectionHandler
    {
        public IGameConnector Game { get; private set; }

        public event Action GameDisconnected;

        public event Action<int> HintTokensEarned;

        private readonly Thread _gameWatchingThread = null;
        private readonly object _gameLock = new object();
        private readonly Dispatcher _dispatcher = null;

        // ------------------------------------

        public GameConnectionHandler(IGameConnector game, Dispatcher dispatcher)
        {
            Game = game;
            _dispatcher = dispatcher;

            // Setup a timer that will trigger a tick every 100ms to poll the currently connected game
            _gameWatchingThread = new Thread(() => {
                while (true)
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
            _gameWatchingThread.Abort();
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
                        HintTokensEarned?.Invoke(totalObtainedHintTokens);
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
