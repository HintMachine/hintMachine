using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    class BustAMove4Connector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private sbyte _previousWins = 0;
        private readonly HintQuest _WinQuest = new HintQuest("Wins", 2);
        private long baseAddr = 0;
        private long winAddr = 0;


        public BustAMove4Connector() {
            quests.Add(_WinQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("ePSXe");
            if (!_ram.TryConnect())
                return false;

            winAddr = _ram.baseAddress + 0x62634A;
            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override string GetDescription()
        {
            return "Match 3 gems of the same color and flood your opponent with combos.\n\n" +
                   "Tested on European ROM on ePSXe 1.7.0.";
        }

        public override string GetDisplayName()
        {
            return "Bust a Move 4 (PS1)";
        }

        public override bool Poll()
        {
            sbyte nbWins =_ram.ReadInt8(winAddr);
            if (nbWins > _previousWins)
            {
                _WinQuest.Add(nbWins - _previousWins);
            }
            _previousWins = nbWins;
            
            return true;
        }
    }
}
