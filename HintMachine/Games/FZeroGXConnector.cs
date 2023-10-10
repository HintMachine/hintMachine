using System.Collections.Generic;
using System.Linq;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    public class FZeroGXConnector : IGameConnector
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
            GoalValue = 10,
            MaxIncrease = 2,
        };

        private ProcessRamWatcher _ram = null;
        private long _mem1Addr = 0;

        // ----------------------------------------------------------

        public FZeroGXConnector()
        {
            Name = "F-Zero GX";
            Description = "F-Zero GX is the fourth installment in the F-Zero series and the successor to F-Zero X. " +
                          "The game continues the series' difficult, high-speed racing style, retaining the basic gameplay and control system from the Nintendo 64 title. " +
                          "A heavy emphasis is placed on track memorization and reflexes, which aids in completing the title.";
            SupportedVersions = "Tested on a US ISO on Dolphin 5 (64-bit).";

            Quests.Add(_cupPointsQuest);
            Quests.Add(_knockoutsQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dolphin");
            if (!_ram.TryConnect())
                return false;

            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x20000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                byte[] SIGNATURE = new byte[] { 0x47, 0x46, 0x5A, 0x45, 0x30, 0x31 }; // GFZE01
                byte[] signatureBytes = _ram.ReadBytes(region.BaseAddress, SIGNATURE.Length);
                if (Enumerable.SequenceEqual(signatureBytes, SIGNATURE))
                {
                    _mem1Addr = region.BaseAddress;
                    break;
                }
            }

            return (_mem1Addr != 0);
        }

        public override void Disconnect()
        {
            _ram = null;
            _mem1Addr = 0;
        }

        public override bool Poll()
        {
            long isRacingAddr = _mem1Addr + 0x17D7A9;
            if(_ram.ReadUint8(isRacingAddr) == 1)
            {
                long knockoutsCountAddr = _mem1Addr + 0xC46A22;
                int knockoutsCount = _ram.ReadUint8(knockoutsCountAddr);
                _knockoutsQuest.UpdateValue(knockoutsCount);

                long cupPointsAddr = _mem1Addr + 0x378668;
                _cupPointsQuest.UpdateValue(_ram.ReadUint16(cupPointsAddr, true));
            }

            return true;
        }
    }
}
