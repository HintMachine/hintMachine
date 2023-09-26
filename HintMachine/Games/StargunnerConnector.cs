using System;

namespace HintMachine.Games
{
    class StargunnerConnector : IGameConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 500000
        };
        private readonly HintQuestCumulative _creditsQuest = new HintQuestCumulative
        {
            Name = "Credits",
            GoalValue = 5000,
            Description = "Collect green gems to obtain credits"
        };

        private ProcessRamWatcher _ram = null;
        private long _baseAddr = 0;
        private long _livesAddr = 0;
        private long _scoreAddr = 0;
        private long _creditsAddr = 0;
        private long _creditsShopAddr = 0;
        private bool _gameStarted = false;

        public StargunnerConnector()
        {
            quests.Add(_scoreQuest);
            quests.Add(_creditsQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("DOSBox");
            if (!_ram.TryConnect())
                return false;
            
            _baseAddr = _ram.ReadInt64(0x1D4A380);
            _livesAddr = _baseAddr + 0x5914C;
            _scoreAddr = _baseAddr + 0x59160;
            _creditsAddr = _baseAddr + 0x59168;
            _creditsShopAddr = _baseAddr + 0x58DC8;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override string GetDescription()
        {
            return "Blast your enemies with a huge variety of wepons in this side-scrolling shooter." +
                   "Tested on up-to-date GOG version.";
        }

        public override string GetDisplayName()
        {
            return "Stargunner (GOG)";
        }

        public override bool Poll()
        {
            uint livesNum = _ram.ReadUint32(_livesAddr);
            uint creditsShopNum = _ram.ReadUint32(_creditsShopAddr);
            if (livesNum <= 10 && creditsShopNum == 1500 && !_gameStarted)
            {
                _gameStarted = true;
                Logger.Debug("Start of game !");
            }
            if ((livesNum == 0 || livesNum > 10) && _gameStarted)
            {
                _gameStarted = false;
                Logger.Debug("End of game !");
            }

            if (_gameStarted)
            {
                _scoreQuest.UpdateValue(_ram.ReadUint32(_scoreAddr));
                _creditsQuest.UpdateValue(_ram.ReadUint32(_creditsAddr));
            }
            return true;
        }
    }
}
