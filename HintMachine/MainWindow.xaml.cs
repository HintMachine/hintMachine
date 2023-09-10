using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using Archipelago.MultiClient.Net.Models;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ArchipelagoHintSession _archipelagoSession;
        private readonly IGameConnector _game;
        private readonly Timer _timer;

        public MainWindow(ArchipelagoHintSession archipelagoSession, IGameConnector game)
        {
            InitializeComponent();
            Title = "Hint Machine - " + game.GetDisplayName();

            _archipelagoSession = archipelagoSession;
            _game = game;

            foreach (HintQuest quest in game.quests)
            {
                quest.InitComponents(questsGrid);
                quest.UpdateComponents();
            }

            _timer = new Timer { AutoReset = true, Interval = 100 };
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            Log("Hint Machine is connected to " + game.GetDisplayName());
            Log("You can start playing to complete objectives on the left panel and get random hints on your world.");
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _game.Poll();

            foreach(HintQuest quest in _game.quests)
            {
                if (quest.CheckCompletion())
                {
                    string hint = _archipelagoSession.GetOneRandomHint();
                    if(hint.Length != 0)
                        Log(hint);
                }

                Dispatcher.Invoke(() => { quest.UpdateComponents(); });
            }
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
            Dispatcher.Invoke(() => { messageLog.Text += message + "\n"; });
        }
    }
}
