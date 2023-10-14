namespace HintMachine.Games
{
    public class ColumnsConnector : IMegadriveConnector
    {
        private readonly HintQuestCumulative _jewelsQuest = new HintQuestCumulative
        {
            Name = "Jewels (Arcade)",
            GoalValue = 100,
            MaxIncrease = 10,
        };

        public ColumnsConnector() : base()
        {
            Name = "Columns";
            Description = "Go back in time to a bygone civilization: the ancient world of Phoenicia. There you will play a simple and captivating game where sparkling, rainbow-coloured jewels drop one after another. According to the ancient merchants, by arranging three or more of the same jewels horizontally, vertically or diagonally, you shall perform miracles.";
            SupportedVersions.Add("Columns (World)");
            CoverFilename = "columns.png";
            Author = "Dinopony";

            Quests.Add(_jewelsQuest);
        }

        public override bool Connect()
        {
            if (!base.Connect())
                return false;

            return FindRamSignature(new byte[] { 0xCC, 0x01, 0xBA, 0xBB }, 0x102C);
        }

        public override bool Poll()
        {
            // Check if we are in arcade mode
            if (_ram.ReadUint8(_megadriveRamBaseAddr + 0x8464) != 0)
                return true;
            
            _jewelsQuest.UpdateValue(_ram.ReadUint16(_megadriveRamBaseAddr + 0xC826));

            return true;
        }
    }
}
