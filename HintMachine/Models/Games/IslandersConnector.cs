using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class IslandersConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "ISLANDERS",
            ModuleName = "mono-2.0-bdwgc.dll",
            Hash = "1D29EAED8E610CE4370DB1B396D4DD7C5FED0DB267D18BF97CA91E865100B71E"
        };

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 1000,
            MaxIncrease = 500,
        };

        private readonly HintQuestCumulative _boosterQuest = new HintQuestCumulative
        {
            Name = "Booster Packs Earned",
            GoalValue = 5,
            MaxIncrease = 2,
            CooldownBetweenIncrements = 15,
        };

        private readonly HintQuestCumulative _islandQuest = new HintQuestCumulative
        {
            Name = "Islands Visited",
            GoalValue = 3,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 60,
        };

        private ProcessRamWatcher _ram = null;

        public IslandersConnector()
        {
            Name = "ISLANDERS";
            Description = "Islanders is a minimalist strategy game about building cities on colorful islands. Explore an infinite number of ever-changing new lands, expand your settlements from sprawling villages to vast cities and enjoy the relaxing atmosphere.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "islanders.png";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
            Quests.Add(_boosterQuest);
            Quests.Add(_islandQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long localGameManagerStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x7521F0, new int[] { 0x210, 0x700, 0x20, 0x5A0 });

            if (localGameManagerStructAddress != 0)
            {
                try
                {
                    long scoreValue = _ram.ReadUint32(localGameManagerStructAddress + 0xD8);
                    long boosterValue = _ram.ReadUint32(localGameManagerStructAddress + 0xB8);
                    long islandValue = _ram.ReadUint32(localGameManagerStructAddress + 0xE8);

                    _scoreQuest.UpdateValue(scoreValue);
                    _boosterQuest.UpdateValue(boosterValue);
                    _islandQuest.UpdateValue(islandValue);
                }
                catch { }
            }

            return true;
        }
    }
}