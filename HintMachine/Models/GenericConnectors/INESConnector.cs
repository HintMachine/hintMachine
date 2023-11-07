using HintMachine.Helpers;

namespace HintMachine.Models.GenericConnectors
{
    public abstract class INESConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;

        public long RomBaseAddress { get; private set; } = 0;

        public long RamBaseAddress { get; private set; } = 0;

        // ---------------------------------------------

        public INESConnector()
        {
            Platform = "NES";
            SupportedEmulators.Add("BizHawk 2.9.1 (NesHawk core)");
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(new BinaryTarget {
                DisplayName = "2.9.1",
                ProcessName = "EmuHawk",
                Hash = "6CE622D4ED4E8460CE362CF35EF67DC70096FEC2C9A174CBEF6A3E5B04F18BCC"
            });

            if (!_ram.TryConnect())
                return false;

            RamBaseAddress = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x1F0, 0x20, 0x10 });
            // RomBaseAddress = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF48, new int[] { 0x8, 0x240, 0x68, 0x10, 0x250, 0x10 });
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
            return "";
            // Hash the 0x800 first ROM bytes as identity
            // byte[] romStart = _ram.ReadBytes(RomBaseAddress, 0x800);
            // using (var sha = SHA256.Create())
            //     return BitConverter.ToString(sha.ComputeHash(romStart)).Replace("-", "");
        }
    }
}