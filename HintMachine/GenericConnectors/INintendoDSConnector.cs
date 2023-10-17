using System.Security.Cryptography;
using System;

namespace HintMachine.GenericConnectors
{
    public abstract class INintendoDSConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;

        public long RomBaseAddress { get; private set; } = 0;

        public long RamBaseAddress { get; private set; } = 0;

        // ---------------------------------------------

        public INintendoDSConnector()
        {
            Platform = "DS";
            SupportedEmulators.Add("BizHawk 2.9.1 (MelonDS core)");
        }

        public override bool Connect()
        {
            _ram = new MegadriveRamAdapter(new BinaryTarget
            {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            RomBaseAddress = 0x36F01D51E20; // or 0x36F00B36680
            RamBaseAddress = 0x36F01952020;

            if (!TestRomIdentity())
            {
                Logger.Debug($"Invalid ROM with identity {CurrentROM}");
                return false;
            }

            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
            RomBaseAddress = 0;
            RamBaseAddress = 0;
        }

        public override long GetCurrentFrameCount()
        {
            long framecountAddr = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x200, 0x10, 0x38 });
            return _ram.ReadUint32(framecountAddr);
        }

        public override string GetRomIdentity()
        {
            // Hash the relevant part of the ROM header
            byte[] bytes = _ram.ReadBytes(RomBaseAddress, 0xB0);
            using (var sha = SHA256.Create("System.Security.Cryptography.SHA256Cng"))
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }
    }
}
