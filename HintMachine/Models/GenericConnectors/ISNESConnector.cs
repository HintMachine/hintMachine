using System;
using System.Linq;
using System.Security.Cryptography;
using HintMachine.Helpers;

namespace HintMachine.Models.GenericConnectors
{
    public abstract class ISNESConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;

        public long RomBaseAddress { get; private set; } = 0;

        public long RamBaseAddress { get; private set; } = 0;

        // ---------------------------------------------

        public ISNESConnector()
        {
            Platform = "SNES";
            SupportedEmulators.Add("BizHawk 2.9.1 (Snes9x core)");
        }

        protected override bool Connect()
        {
            _ram = new MegadriveRamAdapter(new BinaryTarget {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            // It appears that this fixed address is also working, but the memory region method looks just as stable
            // and might work for subsequent versions of Bizhawk as well
            // RamBaseAddress = 0x36F003BD2A0;
            var regions = _ram.ListMemoryRegions().Where(r => r.Type == MemoryRegionType.MEM_MAPPED)
                                                  .Where(r => r.Size == 0x254D000)
                                                  .ToList();
            if (regions.Count > 0)
            {
                RomBaseAddress = regions[0].BaseAddress + 0x1955000;
                RamBaseAddress = regions[0].BaseAddress + 0x3BD2A0;
                Logger.Debug($"RomBaseAddress = 0x{RomBaseAddress:X}");
                Logger.Debug($"RamBaseAddress = 0x{RamBaseAddress:X}");
            }
            else
                return false;

            return true;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _ram = null;

            RamBaseAddress = 0;
            RomBaseAddress = 0;
        }

        public override long GetCurrentFrameCount()
        {
            return BizhawkHelper.GetCurrentFrameCount(_ram);
        }

        public override string GetRomIdentity()
        {
            // We could have used the SNES ROM Header, but this one is placed differently depending on the game ROM type
            // (LoROM, HiROM, ExHiROM) -> cf. https://snes.nesdev.org/wiki/ROM_header
            // Instead, we choose to hash the 2 first MB of the ROM, since it will include the ROM header for most games
            // and should be enough to spot a difference for games that are bigger than this.
            byte[] rom = _ram.ReadBytes(RomBaseAddress, 0x20000);
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(rom)).Replace("-", "");
        }
    }
}
