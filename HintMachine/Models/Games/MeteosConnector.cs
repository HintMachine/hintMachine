using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class MeteosConnector : INintendoDSConnector
    {
        private readonly HintQuestCumulative _sendBlocksQuest = new HintQuestCumulative
        {
            Name = "Send meteos into space",
            Description = "Use simple and deluge mode for this quest",
            GoalValue = 500,
            MaxIncrease = 100
        };

        private readonly HintQuestCumulative _starTripQuest = new HintQuestCumulative
        {
            Name = "Finish Star Trip",
            GoalValue = 1,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 180,
        };

        /* Might be added back later
        private readonly HintQuestCumulative _levelClearQuest = new HintQuestCumulative
        {
            Name = "Clear non-boss Star Trip levels",
            GoalValue = 6,
            MaxIncrease = 6
        };
        */

        private bool _starTripStarted = false;

        // ---------------------------------------------------------

        public MeteosConnector()
        {
            Name = "Meteos";
            Description = "An evil planet named Meteo is sending storms of world-ending meteors across the galaxy, and only your puzzle skills can stop them. As blocks drop down on the lower screen, you must use the DS's stylus to match up blocks of the same color. Once you have enough blocks connected, they'll shoot back into the sky to form planets on the upper screen.\r\n\r\nMatch 3+ blocks of the same color horizontally or vertically to ignite a propulsion and send them to your opponents.";
            SupportedVersions.Add("US ROM");
            CoverFilename = "meteos.png";
            Author = "CalDrac";

            Quests.Add(_sendBlocksQuest);
            Quests.Add(_starTripQuest);
            // Quests.Add(_levelClearQuest); -- Might be added back later

            ValidROMs.Add("85E4BF420FCC466F3CF9A5DA40471DF2789D0FE57461D0B0689C688F6A242D6C"); // NTSC-U
        }

        protected override bool Poll()
        {
            long level = _ram.ReadInt32(RamBaseAddress + 0x3BFEFC);
            if (level == 1)
            {
                _starTripStarted = true;
                _starTripQuest.UpdateValue(0);
            }
            if (level == 16)
            {
                // level = 16 is the ending
                if (_starTripStarted)
                { 
                    _starTripQuest.UpdateValue(1);
                    _starTripStarted = false;
                }
            }
            if(level > 16)
            {
                _starTripStarted = false;

                uint statsPointer = _ram.ReadUint32(RamBaseAddress + 0x1091DC) - 0x2000000;
                if (statsPointer < 0x1000000)
                {
                    uint sentBlocksAddr = statsPointer - 0x20;
                    _sendBlocksQuest.UpdateValue(_ram.ReadUint32(RamBaseAddress + sentBlocksAddr));
                }
            }

            return true;

            /* levelQuest might be added back later
            if (level < 16 && level > 0)
            {
                //Prevent quest increase if retrying a level
                if (level > maxLevel)
                {
                    maxLevel = level;
                }
                //Reset Star Trip condition
                if (level == 1)
                {
                    maxLevel = 1;
                }
                //update nbNiveaux in Star trip
                _levelClearQuest.UpdateValue(maxLevel - 1);
                return true;
            }*/
        }
    }
}
