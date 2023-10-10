namespace HintMachine.Games
{
    public class IslandersConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Cumulative Score",
            GoalValue = 500,
        };

        private ProcessRamWatcher _ram = null;

        public IslandersConnector()
        {
            Name = "ISLANDERS";
            Description = "Minimalist strategy game about building cities on colorful islands";
            SupportedVersions = "Steam";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("ISLANDERS", "mono-2.0-bdwgc.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long scoreAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x7325C8, new int[] { 0xD8, 0xB8, 0x110, 0x30, 0x4D4, 0x40, 0x0 });
            _scoreQuest.UpdateValue(_ram.ReadUint32(scoreAddress + 0x54));

            return true;
        }
    }
}