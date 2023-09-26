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
            Name = "Bust a Move 4 (PS1)";
            Description = "Match 3 gems of the same color and flood your opponent with combos.\n\n" +
                          "Tested on European ROM on ePSXe 1.7.0.";
            Quests.Add(_winQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("ePSXe");
            if (!_ram.TryConnect())
                return false;

            _winAddr = _ram.BaseAddress + 0x62634A;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            _winQuest.UpdateValue(_ram.ReadInt8(_winAddr));
            return true;
        }
    }
}
