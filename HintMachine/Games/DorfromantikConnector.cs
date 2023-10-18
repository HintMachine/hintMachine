using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class DorfromantikConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 1000,
            MaxIncrease = 500,
        };

        private readonly HintQuestCumulative _questQuest = new HintQuestCumulative
        {
            Name = "Quests Fulfilled",
            GoalValue = 10, 
            MaxIncrease = 5,
            CooldownBetweenIncrements = 20,
        };

        private readonly HintQuestCumulative _perfectQuest = new HintQuestCumulative
        {
            Name = "Perfect Tile Placements",
            GoalValue = 10,
            MaxIncrease = 3,
            CooldownBetweenIncrements = 20,
        };

        private ProcessRamWatcher _ram = null;

        private long? _bufferedTileValue = null;

        public DorfromantikConnector()
        {
            Name = "Dorfromantik";
            Description = "Dorfromantik is a peaceful building strategy and puzzle game where you create a beautiful and ever-growing village landscape by placing tiles. Explore a variety of colorful biomes, discover and unlock new tiles and complete quests to fill your world with life!";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "dorfromantik.png";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
            Quests.Add(_questQuest);
            Quests.Add(_perfectQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dorfromantik", "mono-2.0-bdwgc.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long rewardSystemStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x716018, new int[] { 0x8, 0x10, 0x48, 0x18, 0xB0, 0x30, 0x0 });

            if (rewardSystemStructAddress != 0)
            {
                try
                {
                    long tileValue = _ram.ReadUint32(rewardSystemStructAddress + 0x104);
                    long scoreValue = _ram.ReadUint32(rewardSystemStructAddress + 0x100);
                    long questValue = _ram.ReadUint32(rewardSystemStructAddress + 0x114);
                    long perfectValue = _ram.ReadUint32(rewardSystemStructAddress + 0x110);

                    if (_bufferedTileValue == null) { _bufferedTileValue = tileValue; }

                    if (tileValue < (long)_bufferedTileValue)
                    {
                        _scoreQuest.IgnoreNextValue();
                        _questQuest.IgnoreNextValue();
                        _perfectQuest.IgnoreNextValue();
                    }

                    _scoreQuest.UpdateValue(scoreValue);
                    _questQuest.UpdateValue(questValue);
                    _perfectQuest.UpdateValue(perfectValue);

                    _bufferedTileValue = tileValue;
                }
                catch { }
            }

            return true;
        }
    }
}