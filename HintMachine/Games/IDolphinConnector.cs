using System.Collections.Generic;
using System.Linq;
using System.Text;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    public abstract class IDolphinConnector : IGameConnector
    {
        protected ProcessRamWatcher _ram = null;
        protected long _mem1Addr = 0;
        protected long _mem2Addr = 0;
        protected readonly bool _isWii;
        protected readonly string _gameCode;

        // ----------------------------------------------------------------

        public IDolphinConnector(bool isWii, string gameCode)
        {
            _isWii = isWii;
            _gameCode = gameCode;
            // TODO: Platform
            // TODO: SupportedEmulators
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dolphin");
            if (!_ram.TryConnect())
                return false;

            if (!InitMEM1())
                return false;

            if (_isWii && !InitMEM2())
                return false;

            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
            _mem1Addr = 0;
            _mem2Addr = 0;
        }

        protected bool InitMEM1()
        {
            byte[] signature = Encoding.ASCII.GetBytes(_gameCode);

            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x2000000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                byte[] signatureBytes = _ram.ReadBytes(region.BaseAddress, signature.Length);
                if (Enumerable.SequenceEqual(signatureBytes, signature))
                {
                    _mem1Addr = region.BaseAddress;
                    break;
                }
            }

            return (_mem1Addr != 0);
        }

        protected bool InitMEM2()
        {
            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x4000000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                // byte[] signatureBytes = _ram.ReadBytes(region.BaseAddress + signatureOffset, signature.Length);
                // if (Enumerable.SequenceEqual(signatureBytes, signature))
                // {
                    _mem2Addr = region.BaseAddress;
                    break;
                //}
            }

            return (_mem2Addr != 0);
        }
    }
}
