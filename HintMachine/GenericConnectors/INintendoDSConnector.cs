using System.Collections.Generic;
using System.Linq;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.GenericConnectors
{
    public abstract class INintendoDSConnector : IGameConnector
    {
        protected ProcessRamWatcher _ram = null;
        protected long _dsRamBaseAddress = 0;

        public INintendoDSConnector()
        {
            Platform = "DS";
            SupportedEmulators.Add("BizHawk 2.9.1 (MelonDS core)");
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
            _dsRamBaseAddress = 0;
        }

        protected bool FindRamSignature(byte[] ramSignature, uint signatureLookupAddr)
        {
            long ramBaseAddress = 0x36F01952020;
            byte[] signatureBytes = _ram.ReadBytes(ramBaseAddress + signatureLookupAddr, ramSignature.Length);
            if (Enumerable.SequenceEqual(signatureBytes, ramSignature))
            {
                _dsRamBaseAddress = ramBaseAddress;
                return true;
            }

            return false;
        }
    }
}
