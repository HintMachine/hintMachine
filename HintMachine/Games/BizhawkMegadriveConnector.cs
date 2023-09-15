using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace HintMachine.Games
{
    public class BizhawkMegadriveConnector : IGameConnectorProcess
    {
        public BizhawkMegadriveConnector() : base("EmuHawk")
        {}

        public override string GetDisplayName()
        {
            return "Bizhawk";
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

        private long _memAddr = 0;
        private readonly byte[] MEM_SIGNATURE = new byte[] { 0x6E, 0x69, 0x74, 0x69 };

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
                    if ((long)info.RegionSize == 0x2C000)
                    {
                        long ramBaseAddress = (long)info.BaseAddress + 0x5D90;
                        if (Enumerable.SequenceEqual(ReadBytes(ramBaseAddress + 0xFFFC, MEM_SIGNATURE.Length), MEM_SIGNATURE))
                        {
                            _memAddr = ramBaseAddress;
                            return true;
                        }
                    }
                }

                ptr = (IntPtr)(ptr.ToInt64() + (long)info.RegionSize);
            }

            return false;
        }

        [DllImport("threadstack-finder.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern ulong getThreadstack0(ulong pid);

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            Console.WriteLine("X Pos = " + ReadUint16(_memAddr + 0xAC12));

            return true;
        }
    }
}
