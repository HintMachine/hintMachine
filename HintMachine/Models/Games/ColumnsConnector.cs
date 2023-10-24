using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class ColumnsConnector : IMegadriveConnector
    {
        private readonly HintQuestCumulative _jewelsQuest = new HintQuestCumulative
        {
            Name = "Jewels (Arcade)",
            GoalValue = 100,
            MaxIncrease = 10,
        };

        // ----------------------------------------------------------

        public ColumnsConnector() : base()
        {
            Name = "Columns";
            Description = "Go back in time to a bygone civilization: the ancient world of Phoenicia. There you will play a simple and captivating game where sparkling, rainbow-coloured jewels drop one after another. According to the ancient merchants, by arranging three or more of the same jewels horizontally, vertically or diagonally, you shall perform miracles.";
            SupportedVersions.Add("Columns (World)");
            CoverFilename = "columns.png";
            Author = "Dinopony";

            Quests.Add(_jewelsQuest);

            // REV0
            ValidROMs.Add("D84F3E1BBEE9CE8E7FE1401DE9964DB505E3EE41C81A48664983D7A9D39084C8");
            // REV1
            ValidROMs.Add("A04711E2F511C03D41928091877FE7813EC1049662740962BED76B96CEDD9E9C");
        }

        protected override bool Poll()
        {
            bool inArcadeMode = (_ram.ReadUint8(RamBaseAddress + 0x8464) == 0);
            if (inArcadeMode)
                _jewelsQuest.UpdateValue(_ram.ReadUint16(RamBaseAddress + 0xC826));
            
            return true;
        }
    }
}
