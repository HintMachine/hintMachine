namespace HintMachine.Games
{
    public class FZeroGXConnector : IDolphinConnector
    {
        private readonly HintQuestCumulative _cupPointsQuest = new HintQuestCumulative
        {
            Name = "Cup points",
            GoalValue = 200,
            MaxIncrease = 100,
            TimeoutBetweenIncrements = 60
        };

        private readonly HintQuestCumulative _knockoutsQuest = new HintQuestCumulative
        {
            Name = "Knock-outs",
            GoalValue = 20,
            MaxIncrease = 3,
        };

        private bool _wasRacingLastTick = false;

        // ----------------------------------------------------------

        public FZeroGXConnector() : base(false, "GFZE01")
        {
            Name = "F-Zero GX";
            Description = "F-Zero GX is the fourth installment in the F-Zero series and the successor to F-Zero X. The game continues the series' difficult, high-speed racing style, retaining the basic gameplay and control system from the Nintendo 64 title. A heavy emphasis is placed on track memorization and reflexes, which aids in completing the title. GX introduces a \"Story Mode\" element, where the player walks in the footsteps of Captain Falcon through nine chapters, completing various missions.";
            Platform = "GameCube";
            SupportedVersions.Add("NTSC US");
            SupportedEmulators.Add("Dolphin 5");
            CoverFilename = "fzero_gx.png";
            Author = "Dinopony";

            Quests.Add(_cupPointsQuest);
            Quests.Add(_knockoutsQuest);
        }

        public override bool Poll()
        {
            long isRacingAddr = _mem1Addr + 0x17D7A9;
            bool isRacing = (_ram.ReadUint8(isRacingAddr) != 0);

            if (_wasRacingLastTick)
            {
                long knockoutsCountAddr = _mem1Addr + 0xC46A22;
                int knockoutsCount = _ram.ReadUint8(knockoutsCountAddr);
                _knockoutsQuest.UpdateValue(knockoutsCount);

                long cupPointsAddr = _mem1Addr + 0x378668;
                _cupPointsQuest.UpdateValue(_ram.ReadUint16(cupPointsAddr, true));
            }

            _wasRacingLastTick = isRacing;

            return true;
        }
    }
}
