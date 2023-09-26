
using System;

namespace HintMachine.Games
{
    class Rollcage2Connector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private long _placeAddr = 0;
        private long _lapsAddr = 0;
        bool isFirst = false;
        bool raceStarted = false;

        private readonly HintQuestCounter _firstPlacesQuest = new HintQuestCounter
        {
            Name = "Finish first",
            GoalValue = 4
        };

        public Rollcage2Connector()
        {
            quests.Add(_firstPlacesQuest);
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

        public override string GetDisplayName()
        {
            return "Rollcage Stage 2 (PS1)";
        }

        public override string GetDescription()
        {
            return "Race on the floor, on the ceilling, blast your opponents with items and finish first.\n\n" +
                   "Tested on BizHawk version 2.9.1 .";
        }

        public override bool Poll()
        {
            uint laps = _ram.ReadUint8(_lapsAddr);
           
            if (laps == 0) {
                raceStarted = true;
            }
            
            if (laps > 100 && raceStarted) {
                
                if (raceStarted && isFirst) {
                    _firstPlacesQuest.CurrentValue += 1;
                }
                raceStarted = false;
            }
            if (raceStarted)
            {
                uint place = _ram.ReadUint8(_placeAddr);
                isFirst = place == 1;
            }
            return true;
        }
    }
}
