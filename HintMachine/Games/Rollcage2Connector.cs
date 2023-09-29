namespace HintMachine.Games
{
    class Rollcage2Connector : IGameConnector
    {

        private readonly HintQuestCounter _firstPlacesQuest = new HintQuestCounter
        {
            Name = "Finish first",
            GoalValue = 4
        };

        private ProcessRamWatcher _ram = null;
        private long _placeAddr = 0;
        private long _lapsAddr = 0;
        private bool _isFirst = false;
        private bool _raceStarted = false;

        public Rollcage2Connector()
        {
            Name = "Rollcage Stage 2 (PS1)";
            Description = "Race on the floor, on the ceilling, blast your opponents with items and finish first.";
            SupportedVersions = "Tested on BizHawk version 2.9.1.";

            Quests.Add(_firstPlacesQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk");
            if (!_ram.TryConnect())
                return false;
            _placeAddr = 0x36F003A36A1;
            _lapsAddr =  0x36F003A36A0;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            uint laps = _ram.ReadUint8(_lapsAddr);
           
            if (laps == 0) {
                _raceStarted = true;
            }
            
            if (laps > 100 && _raceStarted) {
                
                if (_raceStarted && _isFirst) {
                    _firstPlacesQuest.CurrentValue += 1;
                }
                _raceStarted = false;
            }
            if (_raceStarted)
            {
                uint place = _ram.ReadUint8(_placeAddr);
                _isFirst = place == 1;
            }
            return true;
        }
    }
}
