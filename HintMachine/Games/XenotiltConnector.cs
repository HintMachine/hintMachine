namespace HintMachine.Games
{
    public class XenotiltConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 200000000,
        };

        private ProcessRamWatcher _ram = null;

        public XenotiltConnector()
        {
            Name = "Xenotilt";
            Description = "In this three parts pinball table, you will have to complete missions to earn a lot of points.\n\n" +
                          "Tested on the .282 version";
            Quests.Add(_scoreQuest);
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
            _scoreQuest.UpdateValue(_ram.ReadInt64(scoreAddress));

            return true;
        }
    }
}