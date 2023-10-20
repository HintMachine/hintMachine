using System.Security.Cryptography;
using System;
using System.Text;

namespace HintMachine.GenericConnectors
{
    public abstract class IGameboyColorConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;

        public long RomBaseAddress { get; private set; } = 0;

        public long RamBaseAddress { get; private set; } = 0;

        // ---------------------------------------------

        public IGameboyColorConnector()
        {
            Platform = "GBC";
            SupportedEmulators.Add("BizHawk 2.9.1 (Gambatte core)");
        }

        protected override bool Connect()
        {
            _ram = new MegadriveRamAdapter(new BinaryTarget
            {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                ModuleName = "libgambatte.DLL",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            RamBaseAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x7E050, new int[] { 0x278, 0 });
            RomBaseAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x7E050, new int[] { 0x10 });

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
            long framecountAddr = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x200, 0x10, 0x38 });
            return _ram.ReadUint32(framecountAddr);
        }

        public override string GetRomIdentity()
        {
            // Use the game code as identity
            byte[] codeBytes = _ram.ReadBytes(RomBaseAddress + 0x13F, 4);
            byte[] versionBytes = _ram.ReadBytes(RomBaseAddress + 0x144, 2);
            return Encoding.Default.GetString(codeBytes) + Encoding.Default.GetString(versionBytes);
        }
    }
}
