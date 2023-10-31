using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class PuyoPuyo2MDConnector : IMegadriveConnector
    {
        private readonly HintQuestCumulative _poppedPuyosQuest = new HintQuestCumulative
        {
            Name = "Popped Puyos",
            GoalValue = 200,
            MaxIncrease = 30,
        };
        private readonly HintQuestCumulative _chainsQuest = new HintQuestCumulative
        {
            Name = "Chains",
            GoalValue = 30,
            MaxIncrease = 4
        };
        private readonly HintQuestCounter _allClearsQuest = new HintQuestCounter
        {
            Name = "All Clears",
            GoalValue = 2,
        };
        private byte _previousPuyosOnBoard = 0;

        // ----------------------------------------------------------

        public PuyoPuyo2MDConnector() : base()
        {
            Name = "Puyo Puyo 2";
            Description = "The goal of this head-to-head puzzle game is to clear your grid of falling patterns called puyos by forming chains of four or more same-colored puyos in a straight line or one of several geometric patterns.";
            SupportedVersions.Add("NTSC-J (🇯🇵)");
            CoverFilename = "puyo_puyo_2.png";
            Author = "Dinopony";

            Quests.Add(_poppedPuyosQuest);
            Quests.Add(_chainsQuest);
            Quests.Add(_allClearsQuest);

            ValidROMs.Add("33C3F80F36DA94E25F11F1A2FCEBD5DF22E495567754DB47F330855F6DF91430");
        }

        protected override bool Poll()
        {
            bool isDemo = (_ram.ReadUint8(RamBaseAddress + 0xC8D2) == 0);
            if (isDemo)
            {
                _poppedPuyosQuest.IgnoreNextValue();
                _chainsQuest.IgnoreNextValue();
            }

            _poppedPuyosQuest.UpdateValue(_ram.ReadUint16(RamBaseAddress + 0xD098));

            byte currentChain = _ram.ReadUint8(RamBaseAddress + 0xD089);
            if (currentChain > 0)
                currentChain -= 1;
            _chainsQuest.UpdateValue(currentChain);

            byte puyosOnBoard = _ram.ReadUint8(RamBaseAddress + 0x8279);
            ushort currentPairCount = _ram.ReadUint16(RamBaseAddress + 0xA064);

            // If there are no more puyos on board and we are not at the first pair, this means
            // it's an All-Clear!
            if (puyosOnBoard == 0 && _previousPuyosOnBoard > 0 && currentPairCount > 0 && !isDemo)
                _allClearsQuest.CurrentValue += 1;

            _previousPuyosOnBoard = puyosOnBoard;

            return true;
        }
    }
}
