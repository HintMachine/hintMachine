using System;

namespace HintMachine.Games
{
    class MeteosConnector : IGameConnector
    {
        private readonly HintQuestCumulative _sendBlocksQuest = new HintQuestCumulative
        {
            Name = "Send meteos into space",
            Description = "Use simple and deluge mode for this quest",
            GoalValue = 500,
            MaxIncrease = 100
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
        private readonly HintQuestCumulative _starTripQuest = new HintQuestCumulative
        {
            Name = "Finish Star Trip",
            //Description = "Use Star Trip mode for this quest",
            GoalValue = 1,
            MaxIncrease = 2

        };

        private ProcessRamWatcher _ram = null;
        private long _nbPlayersAddr = 0;
        private long _onePlayerGameAddr = 0;
        private long _twoPlayersGameAddr = 0;
        private long _threePlayersGameAddr = 0;
        private long _fourPlayersGameAddr = 0;
        private long _levelAddr = 0;
        private bool starTripStarted = false;
        private long maxLevel = 0;

        public MeteosConnector()
        {
            Name = "Meteos (DS)";
            Description = "Match 3+ blocks of the same color horizontally or vertically to ignite a propulsion and send them to your opponents.";
            SupportedVersions = "Tested on USA rom with BizHawk 2.9.1";
            
            Quests.Add(_sendBlocksQuest);
            //Quests.Add(_levelClearQuest); //Might be added back later
            Quests.Add(_starTripQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk");
            if (!_ram.TryConnect())
                return false;

            _nbPlayersAddr = 0x36F019B50A0;
            _onePlayerGameAddr = 0x36F01B538EC;
            _twoPlayersGameAddr = 0x36F01B53A0C;
            _threePlayersGameAddr = 0x36F01B53B2C;
            _fourPlayersGameAddr = 0x36F01B53C4C;
            _levelAddr = 0x36F01D11F1C;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long _rightAddr = 0x0;

            long level = _ram.ReadInt32(_levelAddr);
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
            //level = 16 is the ending
            if (level == 16) {
                _starTripQuest.UpdateValue(_starTripQuest.CurrentValue +1);  
            }
            if(level > 16)
            {
                starTripStarted = false;
                int nbPlayers = _ram.ReadInt32(_nbPlayersAddr);

                switch (nbPlayers)
                {
                    case 1:
                        _rightAddr = _onePlayerGameAddr;
                        break;

                    case 2:
                        _rightAddr = _twoPlayersGameAddr;
                        break;

                    case 3:
                        _rightAddr = _threePlayersGameAddr;
                        break;

                    case 4:
                        _rightAddr = _fourPlayersGameAddr;
                        break;
                }
                _sendBlocksQuest.UpdateValue(_ram.ReadInt32(_rightAddr));
                return true;
            }
            return true;
        }
    }
}
