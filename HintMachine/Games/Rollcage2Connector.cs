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
            Name = "Rollcage Stage II";
            Description = "Rollcage Stage II is an arcade-style racing game for PlayStation and PC, developed by Attention To Detail, and published by Psygnosis. It is the sequel to Rollcage and was released in 2000. On top of the basic racing concept, the cars can be equipped with weapons, that are picked up on the track as bonuses, which can be used against competing cars. The automobiles themselves, once again, have wheels that are larger than the body of the car thus creating a car that has no 'right way up' and can be flipped and continue to drive.";
            Platform = "PS1";
            SupportedVersions.Add("Any ROM");
            SupportedEmulators.Add("BizHawk 2.9.1");
            CoverFilename = "rollcage_2.png";
            Author = "CalDrac";

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
