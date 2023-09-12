using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintMachine.Games
{
    public class PuyoTetrisConnector : IGameConnectorProcess
    {
        private ushort _previousLineCount = ushort.MaxValue;
        private readonly HintQuest _linesQuest = new HintQuest("Cleared Lines", 200);

        private ushort _previousTetrises = ushort.MaxValue;
        private readonly HintQuest _tetrisesQuest = new HintQuest("Tetrises", 25);

        private ushort _previousTspins = ushort.MaxValue;
        private readonly HintQuest _tspinsQuest = new HintQuest("T-Spins", 40);

        private byte _previousCombos = byte.MaxValue;
        private readonly HintQuest _combosQuest = new HintQuest("Combos", 60);

        private ushort _previousPerfectClears = ushort.MaxValue;
        private readonly HintQuest _perfectClearsQuest = new HintQuest("Perfect Clears", 5);

        private byte _previousBackToBack = byte.MaxValue;
        private readonly HintQuest _backToBackQuest = new HintQuest("Back-to-Back", 30);

        private ushort _previousPoppedPuyos = ushort.MaxValue;
        private readonly HintQuest _poppedPuyosQuest = new HintQuest("Popped Puyos", 500);

        private byte _previousChain = byte.MaxValue;
        private readonly HintQuest _chainsQuest = new HintQuest("Chains", 100);

        private byte _previousAllClears = byte.MaxValue;
        private readonly HintQuest _allClearsQuest = new HintQuest("All Clears", 5);

        public PuyoTetrisConnector() : base("PuyoPuyoTetris"/*, "puyopuyotetris.exe"*/)
        {
            quests = new List<HintQuest>() {
                _linesQuest, _tetrisesQuest, _tspinsQuest, _combosQuest, _perfectClearsQuest,
                _poppedPuyosQuest, _chainsQuest, _allClearsQuest
            };
        }

        public override string GetDisplayName()
        {
            return "Puyo Puyo Tetris";
        }

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            ReadTetrisData();
            ReadPuyoData();

            return true;
        }

        private void ReadTetrisData()
        {
            long baseAddress = module.BaseAddress.ToInt64() + 0x461B20;
            long tetrisDataBaseAddr = ResolvePointerPath(baseAddress, new int[] { 0x378, 0x28, 0x20, 0x30, 0x28, 0xA8, 0x3E8 });

            if (tetrisDataBaseAddr == 0)
            {
                _previousLineCount = 0;
                _previousTetrises = 0;
                _previousTspins = 0;
                _previousCombos = 0;
                _previousPerfectClears = 0;
                _previousBackToBack = 0;
                return;
            }
            
            ushort lineCount = ReadUint16(tetrisDataBaseAddr);
            if (lineCount > _previousLineCount)
                _linesQuest.Add(lineCount - _previousLineCount);
            _previousLineCount = lineCount;

            ushort tetrisCount = ReadUint16(tetrisDataBaseAddr - 0x60);
            if (tetrisCount > _previousTetrises)
                _tetrisesQuest.Add(tetrisCount - _previousTetrises);
            _previousTetrises = tetrisCount;

            ushort tspinCount = ReadUint16(tetrisDataBaseAddr - 0x50);
            if (tspinCount > _previousTspins)
                _tspinsQuest.Add(tspinCount - _previousTspins);
            _previousTspins = tspinCount;

            byte comboCount = ReadUint8(tetrisDataBaseAddr - 0xC);
            if (comboCount > 0)
                comboCount -= 1;
            if (comboCount > _previousCombos)
                _combosQuest.Add(comboCount - _previousCombos);
            _previousCombos = comboCount;

            ushort perfectClearCount = ReadUint16(tetrisDataBaseAddr - 0x54);
            if(perfectClearCount > _previousPerfectClears)
                _perfectClearsQuest.Add(perfectClearCount - _previousPerfectClears);
            _previousPerfectClears = perfectClearCount;

            byte backToBackCount = ReadUint8(tetrisDataBaseAddr + 0xB);
            if(backToBackCount > _previousBackToBack)
                _backToBackQuest.Add(backToBackCount - _previousBackToBack);
            _previousBackToBack = backToBackCount;
        }

        private void ReadPuyoData()
        {
            long baseAddress = module.BaseAddress.ToInt64() + 0x598A20;
            long puyoDataBaseAddr = ResolvePointerPath(baseAddress, new int[] { 0x38, 0x78, 0xE8, 0x28, 0x28, 0xA8, 0x134 });

            if (puyoDataBaseAddr == 0)
            {
                _previousPoppedPuyos = 0;
                _previousChain = 0;
                _previousAllClears = 0;
                return;
            }

            ushort poppedPuyosCount = ReadUint16(puyoDataBaseAddr + 0x154);
            if(poppedPuyosCount > _previousPoppedPuyos)
                _poppedPuyosQuest.Add(poppedPuyosCount - _previousPoppedPuyos);
            _previousPoppedPuyos = poppedPuyosCount;

            byte chainCount = ReadUint8(puyoDataBaseAddr - 0x4);
            if(chainCount > _previousChain)
                _chainsQuest.Add(chainCount - _previousChain);
            _previousChain = chainCount;

            byte allClearsCount = ReadUint8(puyoDataBaseAddr + 0x34);
            if(allClearsCount > _previousAllClears)
                _allClearsQuest.Add(allClearsCount - _previousAllClears);
            _previousAllClears = allClearsCount;
        }
    }
}
