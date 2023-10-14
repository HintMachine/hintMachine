using System.Linq;

namespace HintMachine.Games
{
    public abstract class IPlayStationConnector : IGameConnector
    {
        protected ProcessRamWatcher _ram = null;
        protected long _psxRamBaseAddress = 0;

        public IPlayStationConnector()
        {
            Platform = "PS1";
            SupportedEmulators.Add("BizHawk 2.9.1 (Nymashock core)");
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk");
            if (!_ram.TryConnect())
                return false;

            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
            _psxRamBaseAddress = 0;
        }

        protected bool FindRamSignature(byte[] ramSignature, uint signatureLookupAddr)
        {
            long ramBaseAddress = 0x36F002D12A8;
            byte[] signatureBytes = _ram.ReadBytes(ramBaseAddress + signatureLookupAddr, ramSignature.Length);
            if (Enumerable.SequenceEqual(signatureBytes, ramSignature))
            {
                _psxRamBaseAddress = ramBaseAddress;
                return true;
            }

            return false;
        }
    }
}
