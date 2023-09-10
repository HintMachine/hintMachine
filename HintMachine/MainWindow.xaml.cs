using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ArchipelagoHintSession archipelagoSession;
        IGameConnector game;

        private Timer _timer;

        public MainWindow(ArchipelagoHintSession archipelagoSession, IGameConnector game)
        {
            InitializeComponent();

            this.archipelagoSession = archipelagoSession;
            this.game = game;

            foreach (HintQuest quest in game.quests)
            {
                quest.InitComponents(questsGrid);
                quest.UpdateComponents();
            }

            _timer = new Timer { AutoReset = true, Interval = 100 };
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            game.Poll();

            foreach(HintQuest quest in game.quests)
            {
                if(quest.CheckCompletion())
                    archipelagoSession.getOneRandomHint();

                Dispatcher.Invoke(() =>
                {
                    quest.UpdateComponents();
                });
            }
        }

    }
}
