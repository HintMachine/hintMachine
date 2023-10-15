using System;

namespace HintMachine.Games
{
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
            //Description = "Use Star Trip mode for this quest",
            GoalValue = 1,
            MaxIncrease = 2

        };

        /* Might be added back later
        private readonly HintQuestCumulative _levelClearQuest = new HintQuestCumulative
        {
            Name = "Clear non-boss Star Trip levels",
            //Description = "Use Star Trip mode for this quest",
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
            //Quests.Add(_levelClearQuest); //Might be added back later
            Quests.Add(_starTripQuest);
        }

        public override bool Connect()
        {
            if (!base.Connect()) 
                return false;

            byte[] METEOS_SIG = new byte[] { 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0x47, 0x8B };
            if (FindRamSignature(METEOS_SIG, 0))
                return true;

            return false;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long level = _ram.ReadInt32(_dsRamBaseAddress + 0x3BFEFC);

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
            }
            else */

            if(level == 1)
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

                uint statsPointer = _ram.ReadUint32(_dsRamBaseAddress + 0x1091DC) - 0x2000000;
                if (statsPointer < 0x1000000)
                {
                    uint sentBlocksAddr = statsPointer - 0x20;
                    _sendBlocksQuest.UpdateValue(_ram.ReadUint32(_dsRamBaseAddress + sentBlocksAddr));
                }
            }

            return true;
        }
    }
}
