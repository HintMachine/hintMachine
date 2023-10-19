using System.Collections.Generic;
using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class PuyoTetrisConnector : IGameConnector
    {
        private readonly HintQuestCumulative _linesQuest = new HintQuestCumulative
        {
            Name = "Cleared Lines",
            GoalValue = 200,
        };
        private readonly HintQuestCumulative _tetrisesQuest = new HintQuestCumulative
        {
            Name = "Tetrises",
            GoalValue = 25,
        };
        private readonly HintQuestCumulative _tspinsQuest = new HintQuestCumulative
        {
            Name = "T-Spins",
            GoalValue = 40,
        };
        private readonly HintQuestCumulative _combosQuest = new HintQuestCumulative
        {
            Name = "Combos",
            GoalValue = 60,
        };
//        private readonly HintQuestCumulative _perfectClearsQuest = new HintQuestCumulative
//        {
//            Name = "Perfect Clears",
//            GoalValue = 5,
//        };
        private readonly HintQuestCumulative _backToBackQuest = new HintQuestCumulative
        {
            Name = "Back-to-Back",
            GoalValue = 30,
        };
        private readonly HintQuestCumulative _poppedPuyosQuest = new HintQuestCumulative
        {
            Name = "Popped Puyos",
            GoalValue = 300,
        };
        private readonly HintQuestCumulative _chainsQuest = new HintQuestCumulative
        {
            Name = "Chains",
            GoalValue = 100,
        };
//        private readonly HintQuestCumulative _allClearsQuest = new HintQuestCumulative
//        {
//            Name = "All Clears",
//            GoalValue = 3,
//        };

        private ProcessRamWatcher _ram = null;

        public PuyoTetrisConnector()
        {
            Name = "Puyo Puyo Tetris";
            Description = "Two puzzle game juggernauts collide as Tetris, one of the largest-selling and recognized brands in gaming history, and Puyo Puyo from SEGA have combined to create a fun-to-play, fast-paced, competitive party game like no other! The game has tons of different styles of gameplay – from the single-player Adventure and Challenge modes to the ferocious competition of the up-to-four player Arcade modes. There's plenty of Tetris/Puyo variety, even an in-game Shop where players can unlock new characters, new skins for Puyos and Tetriminos, new backgrounds and more.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "puyo_puyo_tetris.png";
            Author = "Dinopony";

            Quests = new List<HintQuest>() {
                _linesQuest, _tetrisesQuest, _tspinsQuest, _combosQuest, /*_perfectClearsQuest,*/
                _poppedPuyosQuest, _chainsQuest, /*_allClearsQuest*/
            };
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("PuyoPuyoTetris");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            ReadTetrisData();
            ReadPuyoData();
            return true;
        }

        private void ReadTetrisData()
        {
            int[] OFFSETS = new int[] { 0x378, 0x1D8, 0xE0, 0x08, 0x20, 0xC0, 0x140, 0x60, 0x3E8 };
            long tetrisDataBaseAddr = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x461B20, OFFSETS);
            if (tetrisDataBaseAddr == 0)
                return;

            _linesQuest.UpdateValue(_ram.ReadUint16(tetrisDataBaseAddr));
            _tetrisesQuest.UpdateValue(_ram.ReadUint16(tetrisDataBaseAddr - 0x60));
            _tspinsQuest.UpdateValue(_ram.ReadUint16(tetrisDataBaseAddr - 0x50));

            byte comboCount = _ram.ReadUint8(tetrisDataBaseAddr - 0xC);
            if (comboCount > 0)
                comboCount -= 1;
            _combosQuest.UpdateValue(comboCount);

            // _perfectClearsQuest.UpdateValue(_ram.ReadUint16(tetrisDataBaseAddr - 0x54));
            _backToBackQuest.UpdateValue(_ram.ReadUint8(tetrisDataBaseAddr + 0xB));
        }

        private void ReadPuyoData()
        {
            int[] OFFSETS = new int[] { 0x38, 0x78, 0xE8, 0x28, 0x28, 0xA8, 0x134 };
            long puyoDataBaseAddr = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x598A20, OFFSETS);
            if (puyoDataBaseAddr == 0)
                return;

            _poppedPuyosQuest.UpdateValue(_ram.ReadUint16(puyoDataBaseAddr + 0x154));
            _chainsQuest.UpdateValue(_ram.ReadUint8(puyoDataBaseAddr - 0x4));
            // _allClearsQuest.UpdateValue(_ram.ReadUint8(puyoDataBaseAddr + 0x34));
        }
    }
}
