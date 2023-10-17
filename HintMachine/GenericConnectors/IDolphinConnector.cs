using HintMachine.GenericConnectors;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HintMachine.GenericConnectors
{
    public abstract class IDolphinConnector : IEmulatorConnector
    {
        protected ProcessRamWatcher _ram = null;
        protected readonly bool _isWii;

        protected long MEM1 { get; private set; } = 0;

        protected long MEM2 { get; private set; } = 0;

        // ----------------------------------------------------------------

        public IDolphinConnector(bool isWii)
        {
            _isWii = isWii;
            Platform = (isWii) ? "Wii" : "GameCube";
            SupportedEmulators.Add("Dolphin 5");
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Dolphin");
            _ram.IsBigEndian = true;

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
            MEM1 = 0;
            MEM2 = 0;
        }

        private bool InitMEM1()
        {
            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x2000000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                byte[] headerBytes = _ram.ReadBytes(region.BaseAddress, 6);
                foreach (string gameCode in ValidROMs)
                {
                    byte[] gameCodeBytes = Encoding.ASCII.GetBytes(gameCode);
                    
                    if (Enumerable.SequenceEqual(gameCodeBytes, headerBytes))
                    {
                        MEM1 = region.BaseAddress;
                        Logger.Debug($"MEM1 = {MEM1:X}");
                        return true;
                    }
                }
            }

            return false;
        }

        private bool InitMEM2()
        {
            List<MemoryRegion> regions = _ram.ListMemoryRegions(0x4000000, MemoryRegionType.MEM_MAPPED);
            foreach (MemoryRegion region in regions)
            {
                MEM2 = region.BaseAddress;
                Logger.Debug($"MEM2 = {MEM2:X}");
                return true;
            }

            return false;
        }

        public override long GetCurrentFrameCount()
        {
            // Getting a reliable frameCount over many versions of Dolphin is pretty much impossible, so...
            // we don't do it.
            return 0;
        }

        public override string GetRomIdentity()
        {
            // Use the 6 characters long game code as identity
            byte[] headerBytes = _ram.ReadBytes(MEM1, 0x06);
            return Encoding.Default.GetString(headerBytes);
        }
    }
}
