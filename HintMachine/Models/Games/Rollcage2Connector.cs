using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class Rollcage2Connector : IPlayStationConnector
    {
        private readonly HintQuestCounter _firstPlacesQuest = new HintQuestCounter
        {
            Name = "Finish first",
            GoalValue = 4,
            MaxIncrease = 1,
        };

        private bool _isFirst = false;
        private bool _raceStarted = false;

        public Rollcage2Connector()
        {
            Name = "Rollcage Stage II";
            Description = "Rollcage Stage II is an arcade-style racing game for PlayStation and PC, developed by Attention To Detail, and published by Psygnosis. It is the sequel to Rollcage and was released in 2000. On top of the basic racing concept, the cars can be equipped with weapons, that are picked up on the track as bonuses, which can be used against competing cars. The automobiles themselves, once again, have wheels that are larger than the body of the car thus creating a car that has no 'right way up' and can be flipped and continue to drive.";
            SupportedVersions.Add("Any ROM");
            CoverFilename = "rollcage_2.png";
            Author = "CalDrac";

            Quests.Add(_firstPlacesQuest);

            ValidROMs.Add("SLUS_008.67");
        }

        protected override bool Poll()
        {
            uint laps = _ram.ReadUint8(RamBaseAddress + 0xD23F8);
           
            if (laps == 0)
            {
                _raceStarted = true;
            }
            
            if (laps > 100 && _raceStarted)
            {
                if (_isFirst)
                {
                    _firstPlacesQuest.CurrentValue += 1;
                }
                _raceStarted = false;
            }

            if (_raceStarted)
            {
                uint place = _ram.ReadUint8(RamBaseAddress + 0xD23F9);
                _isFirst = (place == 1);
            }

            return true;
        }
    }
}
