namespace HintMachine.Games
{
    public class DorfromantikConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 1000,
            MaxIncrease = 500,
            TimeoutBetweenIncrements = 3,
        };

        private readonly HintQuestCumulative _questQuest = new HintQuestCumulative
        {
            Name = "Quests Fulfilled",
            GoalValue = 10,
            MaxIncrease = 5,
            TimeoutBetweenIncrements = 20,
        };

        private readonly HintQuestCumulative _perfectQuest = new HintQuestCumulative
        {
            Name = "Perfect Tile Placements",
            GoalValue = 10,
            MaxIncrease = 3,
            TimeoutBetweenIncrements = 20,
        };

        private ProcessRamWatcher _ram = null;

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

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dorfromantik", "mono-2.0-bdwgc.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            // long? tilesValuePrevious = null;
            long? scoreValuePrevious = null;
            long? questValuePrevious = null;
            long? perfectValuePrevious = null;

            // long? tilesValue = null;
            long? scoreValue =null;
            long? questValue = null;
            long? perfectValue = null;

            // Backup pointer
            // _ram.ResolvePointerPath64(_ram.BaseAddress + 0x98B9B8, new int[] { 0x28, 0x30, 0x88, 0x20, 0x90, 0x10, 0x20, 0x18, 0x0 });
            long statsStructAddressPrevious = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x84B9A0, new int[] { 0x28, 0x30, 0x88, 0x20, 0x90, 0x10, 0x20, 0x18, 0x0 });

            if (statsStructAddressPrevious > 0)
            {
                try
                {
                    // tilesValuePrevious = _ram.ReadUint32(statsStructAddressPrevious + 0x2C);
                    scoreValuePrevious = _ram.ReadUint32(statsStructAddressPrevious + 0x14);
                    questValuePrevious = _ram.ReadUint32(statsStructAddressPrevious + 0x24);
                    perfectValuePrevious = _ram.ReadUint32(statsStructAddressPrevious + 0x20);
                }
                catch
                { }
            }

            // Backup pointer
            // _ram.ResolvePointerPath64(_ram.BaseAddress + 0x98B9B8, new int[] { 0x28, 0x30, 0x88, 0x20, 0x90, 0x10, 0x28, 0x18, 0x0 });
            long statsStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x84B9A0, new int[] { 0x28, 0x30, 0x88, 0x20, 0x90, 0x10, 0x28, 0x18, 0x0 });

            if (statsStructAddress > 0)
            {
                try
                {
                    // tilesValue = _ram.ReadUint32(statsStructAddress + 0x2C);
                    scoreValue = _ram.ReadUint32(statsStructAddress + 0x14);
                    questValue = _ram.ReadUint32(statsStructAddress + 0x24);
                    perfectValue = _ram.ReadUint32(statsStructAddress + 0x20);

                    _scoreQuest.UpdateValue((long)scoreValue);
                    _questQuest.UpdateValue((long)questValue);
                    _perfectQuest.UpdateValue((long)perfectValue);
                }
                catch
                {}
            }
            else
            {
                if (scoreValuePrevious != null)
                {
                    _scoreQuest.UpdateValue((long)scoreValuePrevious);
                }

                if (questValuePrevious != null)
                {
                    _questQuest.UpdateValue((long)questValuePrevious);
                }

                if (perfectValuePrevious != null)
                {
                    _perfectQuest.UpdateValue((long)perfectValuePrevious);
                }
            }

            return true;
        }
    }
}