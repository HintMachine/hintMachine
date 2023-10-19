using System.Security.Cryptography;
using System;
using System.Text;

namespace HintMachine.GenericConnectors
{
    public abstract class IGameboyAdvanceConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;

        public long RomBaseAddress { get; private set; } = 0;

        public long ExternalRamBaseAddress { get; private set; } = 0;

        public long InternalRamBaseAddress { get; private set; } = 0;

        // ---------------------------------------------

        public IGameboyAdvanceConnector()
        {
            Platform = "GBA";
            SupportedEmulators.Add("BizHawk 2.9.1 (mGBA core)");
        }

        protected override bool Connect()
        {
            _ram = new MegadriveRamAdapter(new BinaryTarget
            {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                ModuleName = "mgba.dll",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            ExternalRamBaseAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x00103448, new int[] { 0x10, 0x28, 0x0 });
            InternalRamBaseAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x00103448, new int[] { 0x10, 0x30, 0x0 });
            RomBaseAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x00103448, new int[] { 0x10, 0x38, 0x0 });

            return true;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _ram = null;

            ExternalRamBaseAddress = 0;
            InternalRamBaseAddress = 0;
            RomBaseAddress = 0;
        }

        public override long GetCurrentFrameCount()
        {
            long framecountAddr = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x200, 0x10, 0x38 });
            return _ram.ReadUint32(framecountAddr);
        }

        public override string GetRomIdentity()
        {
            // Use the 6 characters long game code as identity
            byte[] headerBytes = _ram.ReadBytes(RomBaseAddress + 0xAC, 0x06);
            return Encoding.Default.GetString(headerBytes);
        }
    }
}
