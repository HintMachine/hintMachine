using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace HintMachine.Games
{
    public class GeometryWarsGalaxiesConnector : IGameConnectorProcess
    {
        private uint _previousGeoms = uint.MaxValue;
        private readonly HintQuest _geomsQuest = new HintQuest("Geoms collected", 1000);

        public GeometryWarsGalaxiesConnector() : base("Dolphin")
        {
            quests.Add(_geomsQuest);
        }

        public override string GetDisplayName()
        {
            return "Geometry Wars Galaxies (Wii)";
        }

        public override string GetDescription()
        {
            return "Destroy geometric enemies in this classic and stylish twin-stick shooter in order to earn geoms and unlock hints.\n\n" +
                   "Tested on European ROM on all recent versions of Dolphin 5 (64-bit).";
        }

        public enum TypeEnum : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public TypeEnum Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        private long _mem1Addr = 0;
        private long _mem2Addr = 0;

        private readonly byte[] MEM1_SIGNATURE = new byte[] { 0x52, 0x47, 0x4C, 0x50, 0x37, 0x44 };
        private readonly byte[] MEM2_SIGNATURE = new byte[] { 0x02, 0x9F, 0x00, 0x10, 0x02, 0x9F, 0x00, 0x33 };

        public override bool Connect()
        {
            if (!base.Connect())
                return false;

            MEMORY_BASIC_INFORMATION info = new MEMORY_BASIC_INFORMATION();
            int mbiSize = Marshal.SizeOf(info);

            IntPtr ptr = IntPtr.Zero;
            while (VirtualQueryEx(processHandle, ptr, out info, (uint)mbiSize) == mbiSize)
            {
                if (info.Type == TypeEnum.MEM_MAPPED)
                {
                    if (_mem1Addr == 0 && (long)info.RegionSize == 0x2000000)
                    {
                        long regionBaseAddress = (long)info.BaseAddress;
                        if (Enumerable.SequenceEqual(ReadBytes(regionBaseAddress, MEM1_SIGNATURE.Length), MEM1_SIGNATURE))
                            _mem1Addr = regionBaseAddress;
                    }
                    else if (_mem2Addr == 0 && (long)info.RegionSize == 0x4000000)
                    {
                        long regionBaseAddress = (long)info.BaseAddress;
                        if (Enumerable.SequenceEqual(ReadBytes(regionBaseAddress, MEM2_SIGNATURE.Length), MEM2_SIGNATURE))
                            _mem2Addr = regionBaseAddress;
                    }
                }

                if (_mem1Addr != 0 && _mem2Addr != 0)
                    return true;

                ptr = (IntPtr)(ptr.ToInt64() + (long)info.RegionSize);
            }

            return false;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _mem1Addr = 0;
            _mem2Addr = 0;
        }

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            if (!Enumerable.SequenceEqual(ReadBytes(_mem1Addr, MEM1_SIGNATURE.Length), MEM1_SIGNATURE))
                return false;

            long geomsAddress = _mem2Addr + 0x23A2AF4;

            uint geoms = ReadUint32(geomsAddress, true);
            if (geoms > _previousGeoms)
                _geomsQuest.Add(geoms - _previousGeoms);
            _previousGeoms = geoms;

            return true;
        }
    }
}
