namespace HintMachine.Games
{
    public class XenotiltConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private long _previousScore = long.MaxValue;
        private readonly HintQuest _scoreQuest = new HintQuest("Score", 200000000);

        public XenotiltConnector()
        {
            quests.Add(_scoreQuest);
        }

        public override string GetDisplayName()
        {
            return "Xenotilt";
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Xenotilt", "mono-2.0-bdwgc.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long scoreAddress = _ram.ResolvePointerPath64(_ram.baseAddress + 0x7270B8, new int[] { 0x30, 0x7e0, 0x7C0 });
            
            long score = _ram.ReadInt64(scoreAddress);
            if (score > _previousScore)
                _scoreQuest.Add(score - _previousScore);
            _previousScore = score;

            return true;
        }
    }
}