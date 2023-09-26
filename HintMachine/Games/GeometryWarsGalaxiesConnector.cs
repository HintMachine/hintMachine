using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    public class GeometryWarsGalaxiesConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;

        private readonly HintQuestCumulative _geomsQuest = new HintQuestCumulative
        {
            Name = "Geoms collected",
            GoalValue = 10000
        };
       
        private long _mem1Addr = 0;
        private long _mem2Addr = 0;

        public GeometryWarsGalaxiesConnector()
        {
            quests.Add(_geomsQuest);
        }

        public override string GetDisplayName()
        {
            return "Geometry Wars Galaxies (Wii)";
        }

        public override string GetDescription()
        {
            return "Destroy geometric enemies in this classic and stylish twin-stick shooter in order to earn geoms and unlock hints.\n\n" +
                   "Tested on European ROM on all recent versions of Dolphin 5 (64-bit).";
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dolphin");
            if (!_ram.TryConnect())
                return false;

            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x2000000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                byte[] SIGNATURE = new byte[] { 0x52, 0x47, 0x4C, 0x50, 0x37, 0x44 };
                byte[] signatureBytes = _ram.ReadBytes(region.BaseAddress, SIGNATURE.Length);
                if (Enumerable.SequenceEqual(signatureBytes, SIGNATURE))
                {
                    _mem1Addr = region.BaseAddress;
                    break;
                }
            }

            regions = _ram.ListMemoryRegions(0x4000000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                byte[] SIGNATURE = new byte[] { 0x02, 0x9F, 0x00, 0x10, 0x02, 0x9F, 0x00, 0x33 };
                byte[] signatureBytes = _ram.ReadBytes(region.BaseAddress, SIGNATURE.Length);
                if (Enumerable.SequenceEqual(signatureBytes, SIGNATURE))
                {
                    _mem2Addr = region.BaseAddress;
                    break;
                }
            }

            return (_mem1Addr != 0 && _mem2Addr != 0);
        }

        public override void Disconnect()
        {
            _ram = null;
            _mem1Addr = 0;
            _mem2Addr = 0;
        }

        public override bool Poll()
        {
            long geomsAddr = _mem2Addr + 0x23A2AF4;
            _geomsQuest.UpdateValue(_ram.ReadUint32(geomsAddr, true));

            return true;
        }
    }
}
