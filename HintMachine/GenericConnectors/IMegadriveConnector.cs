using System.Collections.Generic;
using System.Linq;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.GenericConnectors
{
    public abstract class IMegadriveConnector : IGameConnector
    {
        protected ProcessRamWatcher _ram = null;
        protected long _megadriveRamBaseAddr = 0;

        public IMegadriveConnector()
        {
            Platform = "Megadrive";
            SupportedEmulators.Add("BizHawk 2.9.1 (Genesis Plus GX core)");
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
            _megadriveRamBaseAddr = 0;
        }

        protected bool FindRamSignature(byte[] ramSignature, uint signatureLookupAddr)
        {
            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x2C000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                long ramBaseAddress = region.BaseAddress + 0x5D90;
                byte[] signatureBytes = _ram.ReadBytes(ramBaseAddress + signatureLookupAddr, ramSignature.Length);
                if (Enumerable.SequenceEqual(signatureBytes, ramSignature))
                {
                    _megadriveRamBaseAddr = ramBaseAddress;
                    return true;
                }
            }

            return false;
        }
    }
}
