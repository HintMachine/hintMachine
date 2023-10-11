namespace HintMachine.Games
{
    public class PacManChampionshipEditionDXConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Cumulative Score",
            GoalValue = 400000,
        };

        private ProcessRamWatcher _ram = null;

        public PacManChampionshipEditionDXConnector()
        {
            Name = "PAC-MAN Championship Edition DX+";
            Description = "It's time to gear up! Get ready for more ghost chain gobbling and frantic action in PAC-MAN CE-DX+!";
            SupportedVersions = "Steam";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("PAC-MAN");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            _scoreQuest.UpdateValue(_ram.ReadUint32(_ram.BaseAddress + 0x33CA14));
            return true;
        }
    }
}