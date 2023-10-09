using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace HintMachine
{
    public class ProcessRamWatcher
    {
        #region EXTERNAL_DECLARATIONS

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

        public enum MemoryRegionType : uint
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

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError=true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Microsoft.Win32.SafeHandles.SafeAccessTokenHandle OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        /*
        [DllImport("threadstack-finder.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong getThreadstack0(ulong pid);
        */

        #endregion

        // ----------------------------------------------------------------------------------

        private readonly string _processName;
        private readonly string _moduleName;

        private Process _process = null;
        private ProcessModule _module = null;
        private IntPtr _processHandle = IntPtr.Zero;

        public long BaseAddress { get; set; } = 0;

        // ----------------------------------------------------------------------------------

        public ProcessRamWatcher(string processName, string moduleName = "")
        {
            _processName = processName;
            _moduleName = moduleName;
        }

        ~ProcessRamWatcher()
        {
            if(_processHandle != IntPtr.Zero)
                CloseHandle(_processHandle);
        }

        /// <summary>
        /// Try hooking on the process and module with the names given at object construction.
        /// </summary>
        /// <returns>True if connection succeeded, false if something wrong happened</returns>
        public bool TryConnect()
        {
            // Find the process using its name
            Process[] processes = Process.GetProcessesByName(_processName);
            if (processes.Length > 0)
                _process = processes[0];
            if (_process == null)
                return false;

            // Find the module using its name if it was provided, or take the main module otherwise
            if (_moduleName == "")
            {
                _module = _process.MainModule;
            }
            else
            {
                foreach (ProcessModule m in _process.Modules)
                {
                    if (m.FileName.Contains(_moduleName))
                    {
                        _module = m;
                        break;
                    }
                }
            }
            if (_module == null)
                return false;

            BaseAddress = _module.BaseAddress.ToInt64();

            _processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_QUERY_INFORMATION, false, _process.Id);
            return _processHandle != IntPtr.Zero;
        }

        /// <summary>
        /// Read an array of bytes at the given address in memory.
        /// </summary>
        /// <param name="address">the address where to read bytes.</param>
        /// <param name="length">the number of bytes to read at that address.</param>
        /// <param name="isBigEndian">if true, the output array will be reversed to emulate a big endian reading</param>
        /// <returns>An array of bytes containing read data.</returns>
        /// <exception cref="Exception">
        /// Thrown when the object is in a bad state (no attached process) or if data could not be read from the process
        /// for any reason.
        /// </exception>
        public byte[] ReadBytes(long address, int length, bool isBigEndian = false)
        {
            if (_processHandle == IntPtr.Zero)
                throw new Exception("Could not read memory from a ProcessRamWatcher which failed to initialize");
            
            int bytesRead = 0;
            byte[] buffer = new byte[length];
            ReadProcessMemory((int)_processHandle, address, buffer, length, ref bytesRead);
            if (bytesRead < length)
                throw new Exception("Could not read process memory");

            // If data is meant to be read as big endian, reverse it for BitConverter methods to work as they should
            if (length > 1 && isBigEndian)
                Array.Reverse(buffer);

            return buffer;
        }

        public byte ReadUint8(long address) 
            => ReadBytes(address, sizeof(byte))[0];

        public ushort ReadUint16(long address, bool isBigEndian = false) 
            => BitConverter.ToUInt16(ReadBytes(address, sizeof(ushort), isBigEndian), 0);

        public uint ReadUint32(long address, bool isBigEndian = false)
            => BitConverter.ToUInt32(ReadBytes(address, sizeof(uint), isBigEndian), 0);

        public ulong ReadUint64(long address, bool isBigEndian = false)
            => BitConverter.ToUInt64(ReadBytes(address, sizeof(ulong), isBigEndian), 0);

        public sbyte ReadInt8(long address)
            => (sbyte)ReadBytes(address, sizeof(sbyte))[0];

        public short ReadInt16(long address, bool isBigEndian = false)
            => BitConverter.ToInt16(ReadBytes(address, sizeof(short), isBigEndian), 0);

        public int ReadInt32(long address, bool isBigEndian = false)
            => BitConverter.ToInt32(ReadBytes(address, sizeof(int), isBigEndian), 0);

        public long ReadInt64(long address, bool isBigEndian = false)
            => BitConverter.ToInt64(ReadBytes(address, sizeof(long), isBigEndian), 0);

        public double ReadDouble(long address, bool isBigEndian = false)
            => BitConverter.ToDouble(ReadBytes(address, sizeof(long), isBigEndian), 0);

        private long ResolvePointerPath(long baseAddress, int[] offsets, bool is64Bit)
        {
            try
            {
                long addr = baseAddress;
                foreach (int offset in offsets)
                {
                    addr = is64Bit ? ReadInt64(addr) : ReadInt32(addr);
                    if (addr == 0)
                        return 0;

                    addr += offset;
                }
                return addr;
            } 
            catch(Exception)
            {
                return 0;
            }
        }

        public long ResolvePointerPath32(long baseAddress, int[] offsets) => ResolvePointerPath(baseAddress, offsets, false);
        public long ResolvePointerPath64(long baseAddress, int[] offsets) => ResolvePointerPath(baseAddress, offsets, true);

        /// <summary>
        /// List all memory regions following the given search criteria. Each parameter can be left with its default value
        /// not to be used as a criterion during the search
        /// </summary>
        /// <param name="size">the size of the region to look for.</param>
        /// <param name="type">the type of the memory region to look for.</param>
        /// <returns>A list of all memory regions having the properties specified in the input parameters</returns>
        public List<MemoryRegion> ListMemoryRegions(long size = 0, MemoryRegionType type = MemoryRegionType.MEM_UNDEFINED)
        {
            List<MemoryRegion> regions = new List<MemoryRegion>();

            MEMORY_BASIC_INFORMATION info = new MEMORY_BASIC_INFORMATION();
            int mbiSize = Marshal.SizeOf(info);

            IntPtr ptr = IntPtr.Zero;
            while (VirtualQueryEx(_processHandle, ptr, out info, (uint)mbiSize) == mbiSize)
            {
                ptr = (IntPtr)(ptr.ToInt64() + (long)info.RegionSize);

                // Check region type if it was specified
                if (type != MemoryRegionType.MEM_UNDEFINED && type != info.Type)
                    continue;

                // Check region size if it was specified
                if(size != 0 && size != (long)info.RegionSize)
                    continue;

                regions.Add(new MemoryRegion()
                {
                    BaseAddress = (long)info.BaseAddress,
                    Size = (long)info.RegionSize,
                    Type = info.Type,
                });
            }

            return regions;
        }
        
        /*
        public long GetThreadstack0()
        {
            return (long)getThreadstack0((ulong)process.Id);
        }
        */

        /// <summary>
        /// Fetch the infamous THREADSTACK0 address (as specified by the CheatEngine software) to use as a base address
        /// for further data retrieval.
        /// </summary>
        public Task<int> GetThreadstack0Address()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "threadstack.exe",
                    Arguments = _process.Id + "",
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
