using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class MarioBallLandConnector : IGameboyAdvanceConnector
    {
        private readonly HintQuestCumulative _starsQuest = new HintQuestCumulative
        {
            Name = "Collect Stars",
            GoalValue = 2,
            MaxIncrease = 1,
        };

        public MarioBallLandConnector()
        {
            Name = "Mario Pinball Land";
            Description = "Jumping has always helped Mario perform heroic feats, but in Mario Pinball Land, the plumber must learn how to roll to rescue the princess. " +
                "When Bowser kidnaps Peach and escapes to another dimension, a scientist transforms Mario into a ball to chase after the fiend. " +
                "Now you must use your flippers to shoot a much rounder Mario into doors that lead to new areas. In his new form, Mario is also useful for knocking down enemies, picking up special bonuses, and finding power-ups.";
            SupportedVersions.Add("USA ROM");
            Author = "CalDrac";
            CoverFilename = "mario_pinball_land.png";

            Quests.Add(_starsQuest);

            ValidROMs.Add("BMVE01");
        }

        protected override bool Poll()
        {
            _starsQuest.UpdateValue(_ram.ReadUint8(ExternalRamBaseAddress + 0x40020));
            return true;
        }
    }
}
