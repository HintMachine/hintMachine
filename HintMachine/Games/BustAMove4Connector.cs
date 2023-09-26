namespace HintMachine.Games
{
    class BustAMove4Connector : IGameConnector
    {
        private readonly HintQuestCumulative _winQuest = new HintQuestCumulative
        {
            Name = "Wins",
            GoalValue = 3
        };

        private ProcessRamWatcher _ram = null;
        private long _winAddr = 0;

        public BustAMove4Connector()
        {
            quests.Add(_winQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("ePSXe");
            if (!_ram.TryConnect())
                return false;

            _winAddr = _ram.baseAddress + 0x62634A;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override string GetDescription()
        {
            return "Match 3 gems of the same color and flood your opponent with combos.\n\n" +
                   "Tested on European ROM on ePSXe 1.7.0.";
        }

        public override string GetDisplayName()
        {
            return "Bust a Move 4 (PS1)";
        }

        public override bool Poll()
        {
            _winQuest.UpdateValue(_ram.ReadInt8(_winAddr));
            return true;
        }
    }
}
