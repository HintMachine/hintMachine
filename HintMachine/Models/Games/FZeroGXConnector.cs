using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class FZeroGXConnector : IDolphinConnector
    {
        private readonly HintQuestCumulative _cupPointsQuest = new HintQuestCumulative
        {
            Name = "Cup points",
            GoalValue = 200,
            MaxIncrease = 100,
            CooldownBetweenIncrements = 60
        };

        private readonly HintQuestCumulative _knockoutsQuest = new HintQuestCumulative
        {
            Name = "Knock-outs",
            GoalValue = 20,
            MaxIncrease = 3,
        };

        private bool _wasRacingLastTick = false;

        // ----------------------------------------------------------

        public FZeroGXConnector() : base(false)
        {
            Name = "F-Zero GX";
            Description = "F-Zero GX is the fourth installment in the F-Zero series and the successor to F-Zero X. The game continues the series' difficult, high-speed racing style, retaining the basic gameplay and control system from the Nintendo 64 title. A heavy emphasis is placed on track memorization and reflexes, which aids in completing the title. GX introduces a \"Story Mode\" element, where the player walks in the footsteps of Captain Falcon through nine chapters, completing various missions.";
            SupportedVersions.Add("NTSC US");
            SupportedVersions.Add("PAL");
            CoverFilename = "fzero_gx.png";
            Author = "Dinopony";

            Quests.Add(_cupPointsQuest);
            Quests.Add(_knockoutsQuest);

            ValidROMs.Add("GFZE01"); // NTSC-U
            ValidROMs.Add("GFZP01"); // PAL
        }

        protected override bool Poll()
        {
            bool isPALVersion = (CurrentROM == "GFZP01");

            long isRacingAddr = isPALVersion ? 0x17BF7D : 0x17D7A9;
            long knockoutsAddr = isPALVersion ? 0xC10122 : 0xC46A22;
            long cupPointsAddr = isPALVersion ? 0x386D68 : 0x378668;

            bool isRacing = (_ram.ReadUint8(MEM1 + isRacingAddr) != 0);
            if (_wasRacingLastTick)
            {
                byte knockoutsCount = _ram.ReadUint8(MEM1 + knockoutsAddr);
                _knockoutsQuest.UpdateValue(knockoutsCount);

                ushort cupPoints = _ram.ReadUint16(MEM1 + cupPointsAddr);
                _cupPointsQuest.UpdateValue(cupPoints);
            }

            _wasRacingLastTick = isRacing;

            return true;
        }
    }
}
