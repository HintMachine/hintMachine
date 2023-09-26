using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    class StargunnerConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private readonly HintQuest _scoreQuest = new HintQuest("Score", 500000);
        private readonly HintQuest _creditsQuest = new HintQuest("Credits", 5000,"cumulative",1,"Collect green gems to obtain credits");

        private long baseAddr = 0;
        private long _livesAddr = 0;
        private long _scoreAddr = 0;
        private long _creditsAddr = 0;
        private long _creditsShopAddr = 0;
        private bool gameStarted = false;
        private uint _previousScore = 0;
        private uint _previousCredits = uint.MaxValue;

        public StargunnerConnector() {
            quests.Add(_scoreQuest);
            quests.Add(_creditsQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("DOSBox");
            if (!_ram.TryConnect())
                return false;
            
            baseAddr = _ram.ReadInt64(0x1D4A380);
            _livesAddr = baseAddr + 0x5914C;
            _scoreAddr = baseAddr + 0x59160;
            _creditsAddr = baseAddr + 0x59168;
            _creditsShopAddr = baseAddr + 0x58DC8;
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
            if (livesNum <= 10 && creditsShopNum == 1500 && !gameStarted) {
                gameStarted = true;
                Console.WriteLine("Start of game !");
            }
            if ((livesNum == 0 || livesNum > 10) && gameStarted) {
                gameStarted = false;
                Console.WriteLine("End of game !");
            }
            if (gameStarted) {
                uint scoreNum = _ram.ReadUint32(_scoreAddr);
                if(scoreNum  > _previousScore)
                { 
                    _scoreQuest.Add(scoreNum - _previousScore);
                }
                _previousScore = scoreNum;
                uint creditsNum = _ram.ReadUint32(_creditsAddr);
                if (creditsNum >_previousCredits) {
                     _creditsQuest.Add(creditsNum - _previousCredits);
                }
                _previousCredits = creditsNum;

            }
            return true;
        }
    }
}
