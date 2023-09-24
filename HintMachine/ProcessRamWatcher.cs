using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HintMachine
{
    public class ProcessRamWatcher
    {
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Microsoft.Win32.SafeHandles.SafeAccessTokenHandle OpenThread(
           ThreadAccess dwDesiredAccess,
           bool bInheritHandle,
           uint dwThreadId
           );

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        // ----------------------------------------------------------------------------------
        
        private string _processName;
        private string _moduleName;
        public Process process = null;
        public ProcessModule module = null;
        public IntPtr processHandle = IntPtr.Zero;
        public long baseAddress = 0;

        public ProcessRamWatcher(string processName, string moduleName = "")
        {
            _processName = processName;
            _moduleName = moduleName;

            TokenManipulator.AddPrivilege("SeDebugPrivilege");
            TokenManipulator.AddPrivilege("SeSystemEnvironmentPrivilege");
            Process.EnterDebugMode();
        }

        public bool TryConnect()
        {
            Process[] processes = Process.GetProcessesByName(_processName);
            if (processes.Length == 0)
                return false;

            process = processes[0];
            if (process == null)
                return false;

            if (_moduleName == "")
            {
                module = process.MainModule;
            }
            else
            {
                foreach (ProcessModule m in process.Modules)
                {
                    if (m.FileName.Contains(_moduleName))
                    {
                        module = m;
                        break;
                    }
                }
            }

            if (module == null)
                return false;
            baseAddress = module.BaseAddress.ToInt64();

            processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_QUERY_INFORMATION, false, process.Id);
            return processHandle != IntPtr.Zero;
        }

        public byte[] ReadBytes(long address, int length, bool isBigEndian = false)
        {
            if (processHandle == IntPtr.Zero)
            {
                byte[] zeroByteArray = new byte[length];
                for (int i = 0; i < length; ++i)
                    zeroByteArray[i] = 0;
                return zeroByteArray;
            }

            int bytesRead = 0;
            byte[] buffer = new byte[length];
            ReadProcessMemory((int)processHandle, address, buffer, length, ref bytesRead);
            if (bytesRead < length)
                throw new Exception("Could not read process memory");

            // If data is meant to be read as big endian, reverse it for BitConverter methods to work as they should
            if (length > 1 && isBigEndian)
                Array.Reverse(buffer);

            return buffer;
        }

        public byte ReadUint8(long address)
        {
            return ReadBytes(address, sizeof(byte))[0];
        }

        public ushort ReadUint16(long address, bool isBigEndian = false)
        {
            return BitConverter.ToUInt16(ReadBytes(address, sizeof(ushort), isBigEndian), 0);
        }

        public uint ReadUint32(long address, bool isBigEndian = false)
        {
            return BitConverter.ToUInt32(ReadBytes(address, sizeof(uint), isBigEndian), 0);
        }

        public ulong ReadUint64(long address, bool isBigEndian = false)
        {
            return BitConverter.ToUInt64(ReadBytes(address, sizeof(ulong), isBigEndian), 0);
        }

        public sbyte ReadInt8(long address)
        {
            return (sbyte)ReadBytes(address, sizeof(sbyte))[0];
        }

        public short ReadInt16(long address, bool isBigEndian = false)
        {
            return BitConverter.ToInt16(ReadBytes(address, sizeof(short), isBigEndian), 0);
        }

        public int ReadInt32(long address, bool isBigEndian = false)
        {
            return BitConverter.ToInt32(ReadBytes(address, sizeof(int), isBigEndian), 0);
        }

        public long ReadInt64(long address, bool isBigEndian = false)
        {
            return BitConverter.ToInt64(ReadBytes(address, sizeof(long), isBigEndian), 0);
        }

        protected double ReadDouble(long address, bool isBigEndian = false)
        {
            return BitConverter.ToDouble(ReadBytes(address, sizeof(long), isBigEndian), 0);
        }
        public long ResolvePointerPath32(long baseAddress, int[] offsets)
        {
            long addr = baseAddress;
            foreach (int offset in offsets)
            {
                addr = ReadInt32(addr);
                if (addr == 0)
                    break;

                addr += offset;
            }
            return addr;
        }

        public long ResolvePointerPath64(long baseAddress, int[] offsets)
        {
            long addr = baseAddress;
            foreach (int offset in offsets)
            {
                addr = ReadInt64(addr);
                if (addr == 0)
                    break;

                addr += offset;
            }
            return addr;
        }
        public enum MemoryRegionType: uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000,
            MEM_UNDEFINED = 0x0
        }

        public struct MemoryRegion
        {
            public long BaseAddress;
            public long Size;
            public MemoryRegionType Type;
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
            public MemoryRegionType Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        public List<MemoryRegion> ListMemoryRegions(long size = 0, MemoryRegionType type = MemoryRegionType.MEM_UNDEFINED)
        {
            List<MemoryRegion> regions = new List<MemoryRegion>();

            MEMORY_BASIC_INFORMATION info = new MEMORY_BASIC_INFORMATION();
            int mbiSize = Marshal.SizeOf(info);

            IntPtr ptr = IntPtr.Zero;
            while (VirtualQueryEx(processHandle, ptr, out info, (uint)mbiSize) == mbiSize)
            {
                bool validType = (type == info.Type || type == MemoryRegionType.MEM_UNDEFINED);
                bool validSize = (size == (long)info.RegionSize || size == 0);

                if(validType && validSize)
                {
                    regions.Add(new MemoryRegion() {
                        BaseAddress = (long)info.BaseAddress,
                        Size = (long)info.RegionSize,
                        Type = info.Type,
                    });
                }

                ptr = (IntPtr)(ptr.ToInt64() + (long)info.RegionSize);
            }

            return regions;
        }

       [DllImport("threadstack-finder.dll", CallingConvention = CallingConvention.Cdecl)]
       public static extern ulong getThreadstack0(ulong pid);

        public long GetThreadstack0()
        {
            return (long)getThreadstack0((ulong)process.Id);
        }


        public Task<int> getThread0Address()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "threadstack.exe",
                    Arguments = process.Id + "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("THREADSTACK 0 BASE ADDRESS: "))
                {
                    line = line.Substring(line.LastIndexOf(":") + 2);
                    return Task.FromResult(int.Parse(line.Substring(2), System.Globalization.NumberStyles.HexNumber));
                }
            }
            return Task.FromResult(0);
        }
    }
}
