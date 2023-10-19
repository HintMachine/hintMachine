using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    class DragonCrownConnector : IGameConnector
    {
        
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Get score",
            GoalValue = 100000,
            MaxIncrease = 50000
        };
        private readonly HintQuestCumulative _goldQuest = new HintQuestCumulative
        {
            Name = "Get Gold",
            Description = "Only counts gold obtained during the quests",
            GoalValue = 5000,
            MaxIncrease = 3000
        };

        private ProcessRamWatcher _ram = null;
        private long _scoreAddr = 0;
        private long _goldAddr = 0;

        // ---------------------------------------------------------

        public DragonCrownConnector()
        {
            Name = "Dragon's Crown";
            Description = "Dragon's Crown is a multiplayer hack-and-slash beat'em up game with breathtaking visual style, a design built around cooperative play, epic boss fights, and the ability to discover a new adventure in every play session.";
            SupportedVersions.Add("US ROM");
            CoverFilename = "dragons_crown.png";
            Platform = "PS Vita";
            Author = "CalDrac";

            Quests.Add(_scoreQuest);
            Quests.Add(_goldQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("Vita3K");
            if (!_ram.TryConnect())
                return false;

            _scoreAddr = 0x404B3A2A8;
            _goldAddr = 0x404B3A26C;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            uint score = _ram.ReadUint32(_scoreAddr);
            uint gold = _ram.ReadUint32(_goldAddr);

            _scoreQuest.UpdateValue(score);
            _goldQuest.UpdateValue(gold);
            return true;
        }
    }
}
