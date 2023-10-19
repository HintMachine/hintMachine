using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class GeometryWarsConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 50000
        };

        public GeometryWarsConnector()
        {
            Name = "Geometry Wars: Retro Evolved";
            Description = "Geometry Wars: Retro Evolved is a old school style shooter, but remixed for the 21st century with next generation graphics and deep, modern gameplay. Playing is simple: you are a geometric \"ship\" trapped in a grid world, facing off against waves of deadly wanderers, snakes, and repulsars. Your aim is to survive long enough to set a high score!";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "geometry_wars_re.png";
            Author = "CalDrac";

            Quests.Add(_scoreQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("GeometryWars");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long scoreAddress = 0x63C890;
            _scoreQuest.UpdateValue(_ram.ReadUint32(scoreAddress));

            return true;
        }
    }
}