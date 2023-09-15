namespace HintMachine.Games
{
    public class TetrisEffectConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private uint _previousScore = uint.MaxValue;
        private readonly HintQuest _scoreQuest = new HintQuest("Score", 20000);

        public TetrisEffectConnector()
        {
            quests.Add(_scoreQuest);
        }

        public override string GetDisplayName()
        {
            return "Tetris Effect Connected (Steam)";
        }
        public override string GetDescription()
        {
            return "Stack tetrominos and fill lines to clear them in the most visually " +
                   "impressive implementation of Tetris ever made.\n\n" +
                   "Tested on up-to-date Steam version.";
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("TetrisEffect-Win64-Shipping");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            int[] OFFSETS = new int[] { 0x0, 0x20, 0x120, 0x0, 0x42C };
            long scoreAddress = _ram.ResolvePointerPath64(_ram.baseAddress + 0x4ED0440, OFFSETS);
            
            uint score = _ram.ReadUint32(scoreAddress);
            if (score > _previousScore)
                _scoreQuest.Add(score - _previousScore);
            _previousScore = score;

            return true;
        }
    }
}
