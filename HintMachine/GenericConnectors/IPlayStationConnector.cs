using System.Text;

namespace HintMachine.GenericConnectors
{
    public abstract class IPlayStationConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;

        public long RamBaseAddress { get; private set; } = 0;
        
        // ---------------------------------------------

        public IPlayStationConnector()
        {
            Platform = "PS1";
            SupportedEmulators.Add("BizHawk 2.9.1 (Nymashock core)");
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(new BinaryTarget
            {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            RamBaseAddress = 0x36F002D12A8;

            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
            RamBaseAddress = 0;
        }

        public override long GetCurrentFrameCount()
        {
            long framecountAddr = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x200, 0x10, 0x38 });
            return _ram.ReadUint32(framecountAddr);
        }

        public override string GetRomIdentity()
        {
            // Get the game code from RAM
            return Encoding.Default.GetString(_ram.ReadBytes(RamBaseAddress + 0xB8B7, 11));
        }
    }
}
