﻿using System.Runtime.CompilerServices;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class KirbyAvalancheConnector : ISNESConnector
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

        public KirbyAvalancheConnector() : base()
        {
            Name = "Kirby's Avalanche";
            Description = "King Dedede has challenged Kirby and other members of Dream Land to an Avalanche Competition in the Dream Fountain. Kirby, taking on the challenge, has decided to battle his way through the forest and into the Dream Fountain to win the Avalanche Cup from King Dedede.";
            SupportedVersions.Add("NTSC-U (🇺🇸)");
            CoverFilename = "kirby_avalanche.png";
            Author = "Dinopony";

            Quests.Add(_poppedPuyosQuest);
            Quests.Add(_chainsQuest);
            Quests.Add(_allClearsQuest);

            ValidROMs.Add("D2844FD64E211E7BB8A44ABC45C97930EDE17A3C349E965030EF609469686BA6");
        }

        protected override bool Poll()
        {
            // If we are in demo mode / in an invalid gamemode, just return
            //byte stageId = _ram.ReadUint8(RamBaseAddress + 0x294A);
            bool inGame = (_ram.ReadUint8(RamBaseAddress + 0x8C7) > 0);

            byte puyosOnBoard = _ram.ReadUint8(RamBaseAddress + 0x199C);
            ushort generatedPuyoCount = _ram.ReadUint16(RamBaseAddress + 0x288);
            ushort poppedPuyos = _ram.ReadUint16(RamBaseAddress + 0x718);

            // If there are no more puyos on board and we have already popped puyos, this means
            // it's an All-Clear!
            if (puyosOnBoard == 0 && _previousPuyosOnBoard > 0 && poppedPuyos > 0 && inGame)
                _allClearsQuest.CurrentValue += 1;
            _previousPuyosOnBoard = puyosOnBoard;
            
            byte currentChain = _ram.ReadUint8(RamBaseAddress + 0x709);
            if (currentChain > 0)
                currentChain -= 1;
            if (!inGame)
                _chainsQuest.IgnoreNextValue();
            _chainsQuest.UpdateValue(currentChain);

            // If there were only 8 puyos generated by the RNG, this means we're at stage start and nothing
            // must budge
            if (generatedPuyoCount <= 8 || !inGame)
                _poppedPuyosQuest.IgnoreNextValue();
            _poppedPuyosQuest.UpdateValue(poppedPuyos);

            return true;
        }
    }
}