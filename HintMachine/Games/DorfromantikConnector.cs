namespace HintMachine.Games
{
    public class DorfromantikConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Cumulative Score",
            GoalValue = 1000,
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
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dorfromantik", "UnityPlayer.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long scoreAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x1AD1118, new int[] { 0x8, 0x8, 0xD0, 0x138, 0x40, 0x60, 0x180 });
            _scoreQuest.UpdateValue(_ram.ReadUint32(scoreAddress + 0x5C));

            return true;
        }
    }
}