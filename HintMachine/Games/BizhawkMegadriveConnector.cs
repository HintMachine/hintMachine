using System;
using System.Collections.Generic;
using System.Linq;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    public class BizhawkMegadriveConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private long _megadriveRamBaseAddr = 0;

        public BizhawkMegadriveConnector()
        {}

        public override string GetDisplayName()
        {
            return "Bizhawk";
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk");
            if (!_ram.TryConnect())
                return false;

            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x2C000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                byte[] SIGNATURE = new byte[] { 0x6E, 0x69, 0x74, 0x69 };

                long ramBaseAddress = region.BaseAddress + 0x5D90;
                byte[] signatureBytes = _ram.ReadBytes(ramBaseAddress + 0xFFFC, SIGNATURE.Length);
                if (Enumerable.SequenceEqual(signatureBytes, SIGNATURE))
                {
                    _megadriveRamBaseAddr = ramBaseAddress;
                    return true;
                }
            }

            return false;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            Console.WriteLine("X Pos = " + _ram.ReadUint16(_megadriveRamBaseAddr + 0xAC12));
            return true;
        }
    }
}
