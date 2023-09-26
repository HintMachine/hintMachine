namespace HintMachine.Games
{
    public class TetrisEffectConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 20000,
        };

        private ProcessRamWatcher _ram = null;

        public TetrisEffectConnector()
        {
            quests.Add(_scoreQuest);
        }

        public override string GetDisplayName()
        {
            return "Tetris Effect Connected";
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


            _scoreQuest.UpdateValue(_ram.ReadUint32(scoreAddress));

            return true;
        }
    }
}
