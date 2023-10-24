using System;
using System.Linq;
using System.Security.Cryptography;

namespace HintMachine.Models.GenericConnectors
{
    public abstract class IMegadriveConnector : IEmulatorConnector
    {
        protected MegadriveRamAdapter _ram = null;

        public long RomBaseAddress { get; private set; } = 0;

        public long RamBaseAddress { get; private set; } = 0;
        
        // -------------------------------------------------

        public IMegadriveConnector()
        {
            Platform = "Megadrive";
            SupportedEmulators.Add("BizHawk 2.9.1 (Genesis Plus GX core)");
        }

        protected override bool Connect()
        {
            _ram = new MegadriveRamAdapter(new BinaryTarget
            {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            // Find ROM base address
            var regions = _ram.ListMemoryRegions().Where(r => r.Size == 0x3158000 && r.Type == MemoryRegionType.MEM_MAPPED).ToList();
            if (regions.Count == 0)
            {
                Logger.Debug("IMegadriveConnector: Could not find ROM start address");
                return false;
            }
            RomBaseAddress = regions[0].BaseAddress + 0xE58000;

            // Find RAM base address
            regions = _ram.ListMemoryRegions().Where(r => r.Size == 0x2C000 && r.Type == MemoryRegionType.MEM_MAPPED).ToList();
            if (regions.Count == 0)
            {
                Logger.Debug("IMegadriveConnector: Could not find RAM start address");
                return false;
            }
            RamBaseAddress = regions[0].BaseAddress + 0x5D90;
            Logger.Debug($"RamBaseAddress = {RamBaseAddress:X}");

            return true;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _ram = null;
            RomBaseAddress = 0;
            RamBaseAddress = 0;
        }

        public override long GetCurrentFrameCount()
        {
            return 0;
            /*
            try
            {
                long framecountAddr = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x200, 0x10, 0x38 });
                if (framecountAddr == 0)
                {
                    Logger.Debug("Framecount addr is null, skipping checks...");
                    return 0;
                }
                return _ram.ReadUint32(framecountAddr);
            } 
            catch(ProcessRamWatcherException)
            {
                Logger.Debug("RAM couldn't be read following framecount pointer path, skipping checks...");
                return 0;
            }
            */
        }

        public override string GetRomIdentity()
        {
            // Hash the relevant part of the ROM header
            byte[] bytes = _ram.ReadBytes(RomBaseAddress + 0x100, 0xA0);
            using (var sha = SHA256.Create("System.Security.Cryptography.SHA256Cng"))
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }
    }
}
