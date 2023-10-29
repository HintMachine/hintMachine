using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class AdvanceWarsDualStrikeConnector : INintendoDSConnector
    {
        private readonly HintQuestCumulative _basesCapturedQuest = new HintQuestCumulative
        {
            Name = "Bases Captured",
            GoalValue = 5,
            MaxIncrease = 1
        };

        private readonly HintQuestCumulative _unitsDestroyedQuest = new HintQuestCumulative
        {
            Name = "Units Destroyed",
            GoalValue = 15,
            MaxIncrease = 1,
        };

        private const string GAME_VERSION_PAL = "E4189A354066300BA95AA4F86458FB4CD02E69B722E1CF3F1D65EC3CE39EBE4E";
        private const string GAME_VERSION_NTSC_U = "66DA2A57878F367DE4A91A00CC14A2B37713D511E3F24814942C6F0C35A55EB1";
        private const string GAME_VERSION_NTSC_J = "8E1D5E689EA6C9805F2E2414A033EFD552F89ED00E6880AD5427CFC4BB70BE5E";

        // ---------------------------------------------------------

        public AdvanceWarsDualStrikeConnector()
        {
            Name = "Advance Wars: Dual Strike";
            Description = "Advance Wars: Dual Strike is the third installment in the Advance Wars series (first on DS media). Advance Wars is the international title of the Wars video game series, which dates back to the Family Computer game Famicom Wars in 1988.\r\nThe storyline is a continuation of the previous series and is set in the new location of Omega Land. Black Hole has returned under the leadership of a new commander who seeks to give himself eternal life by draining the energy of Omega Land. The Allied Nations struggle to overcome this threat and are eventually joined by several former Black Hole commanding officers in an effort to save the land.";
            CoverFilename = "advance_wars_dual_strike.png";
            Author = "Dinopony";

            SupportedVersions.Add("PAL (🇪🇺)");
            SupportedVersions.Add("NTSC-U (🇺🇸)");
            SupportedVersions.Add("NTSC-J (🇯🇵)");

            Quests.Add(_basesCapturedQuest);
            Quests.Add(_unitsDestroyedQuest);

            ValidROMs.Add(GAME_VERSION_PAL);
            ValidROMs.Add(GAME_VERSION_NTSC_U);
            ValidROMs.Add(GAME_VERSION_NTSC_J);
        }

        protected override bool Poll()
        {
            long medalsDataOffset;
            if (CurrentROM == GAME_VERSION_PAL)
                medalsDataOffset = 0x2BEF28;
            else if (CurrentROM == GAME_VERSION_NTSC_U)
                medalsDataOffset = 0x290A94;
            else /* if (CurrentROM == GAME_VERSION_NTSC_J) */
                medalsDataOffset = 0x2A8270;

            uint basesCaptured = _ram.ReadUint32(RamBaseAddress + medalsDataOffset + 0x3C);
            _basesCapturedQuest.UpdateValue(basesCaptured);

            uint unitsDestroyed = 0;
            for(int i=0; i<26; ++i)
                unitsDestroyed += _ram.ReadUint32(RamBaseAddress + medalsDataOffset + 0x138 + (i * 4));
            _unitsDestroyedQuest.UpdateValue(unitsDestroyed);

            return true;
        }
    }
}
