using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace HintMachine.Models
{
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

    public class BinaryTarget
    {
        /// <summary>
        /// A readable and concise name for the target (e.g. "Steam")
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// The name of the process that is targeted
        /// </summary>
        public string ProcessName { get; set; } = "";

        /// <summary>
        /// A SHA-256 hash of the binary file the process is running
        /// </summary>
        public string Hash { get; set; } = "";

        /// <summary>
        /// The name of the module whose base address will be taken as the BaseAddress for this watcher
        /// once connected. If left blank, the base address for the main module (the binary) will be taken.
        /// </summary>
        public string ModuleName { get; set; } = "";
    }

    public class ProcessRamWatcher
    {

        // ----------------------------------------------------------------------------------

        private Process _process = null;
        private IntPtr _processHandle = IntPtr.Zero;

        public List<BinaryTarget> SupportedTargets { get; private set; } = new List<BinaryTarget>();

        public BinaryTarget CurrentTarget { get; private set; } = null;

        public long BaseAddress { get; private set; } = 0;

        public bool Is64Bit { get; set; } = true;

        public bool IsBigEndian { get; set; } = false;

        public long Threadstack0
        { 
            get { 
                if(_threadstack0 == null)
                    _threadstack0 = GetThreadstack0Address();

                return _threadstack0 ?? 0;
            }
        }
        private long? _threadstack0 = null;

        // ----------------------------------------------------------------------------------

        public ProcessRamWatcher()
        {}
        public ProcessRamWatcher(BinaryTarget target)
        {
            SupportedTargets.Add(target);
        }

        public ProcessRamWatcher(string processName, string moduleName = "")
        {
            SupportedTargets.Add(new BinaryTarget
            {
                ProcessName = processName,
                ModuleName = moduleName,
            });
        }

        ~ProcessRamWatcher()
        {
            if(_processHandle != IntPtr.Zero)
                NativeMethods.CloseHandle(_processHandle);
        }

        /// <summary>
        /// Try hooking on the process and module with the names given at object construction.
        /// </summary>
        /// <returns>True if connection succeeded, false if something wrong happened</returns>
        public bool TryConnect()
        {
            HashSet<string> alreadyProcessedNames = new HashSet<string>();

            foreach (BinaryTarget target in SupportedTargets)
            {
                // Only try connecting once to each process name
                if (alreadyProcessedNames.Contains(target.ProcessName))
                    continue;

                alreadyProcessedNames.Add(target.ProcessName);
                if (!TryConnectToProcess(target.ProcessName))
                    continue;

                // We managed to find a process with a matching name, if there is a target for this process name with no hash, we're gold.
                CurrentTarget = FindTargetWithNameAndHash(target.ProcessName, "");
                if (CurrentTarget == null)
                {
                    // Otherwise, we need to hash the binary and test it against targets' hashes
                    string hash = GetBinaryHash();
                    CurrentTarget = FindTargetWithNameAndHash(target.ProcessName, hash);
                    if (CurrentTarget == null)
                    {
                        Logger.Error($"Found a process with a valid name but an unsupported version ({hash})");
                        return false;
                    }
                }

                // A valid target was found, initialize BaseAddress with the module's base address
                BaseAddress = FindModuleBaseAddress(CurrentTarget?.ModuleName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts opening the process with the given name with specific rights to allow for RAM peeking afterwards.
        /// </summary>
        /// <param name="processName">the name of the process to connect to</param>
        /// <returns>true if it succeeded, false if something went wrong</returns>
        private bool TryConnectToProcess(string processName)
        {
            // Find the process using its name
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
                _process = processes[0];
            if (_process == null)
                return false;

            // Open the process
            const int PROCESS_WM_READ = 0x0010;
            const int PROCESS_QUERY_INFORMATION = 0x0400;
            _processHandle = NativeMethods.OpenProcess(PROCESS_WM_READ | PROCESS_QUERY_INFORMATION, false, _process.Id);
            if (_processHandle == IntPtr.Zero)
                return false;

            return true;
        }

        /// <summary>
        /// Look for a BinaryTarget with the given process name and hash (case-insensitive) inside the list of supported
        /// binary targets.
        /// </summary>
        /// <param name="processName">the process name to look for</param>
        /// <param name="hash">the hash to look for</param>
        /// <returns>A BinaryTarget with the given processName & hash if it could be found, null otherwise</returns>
        private BinaryTarget FindTargetWithNameAndHash(string processName, string hash)
        {
            foreach (BinaryTarget target in SupportedTargets)
            {
                if (target.ProcessName != processName)
                    continue;
                
                if(target.Hash == "" || string.Equals(hash, target.Hash, StringComparison.OrdinalIgnoreCase))
                    return target;
            }

            return null;
        }

        /// <summary>
        /// Find the module using its name if it was provided, or take the main module otherwise
        /// </summary>
        /// <param name="targetName">the name of the module (or an empty string)</param>
        /// <returns>the base address for the given module (or the main module base address if empty)</returns>
        private long FindModuleBaseAddress(string targetName)
        {
            if (targetName == "")
                return _process.MainModule.BaseAddress.ToInt64();

            long result = 0;

            // Setting up the variable for the second argument for EnumProcessModules
            IntPtr[] hMods = new IntPtr[1024];
            GCHandle gch = GCHandle.Alloc(hMods, GCHandleType.Pinned); // Don't forget to free this later
            IntPtr pModules = gch.AddrOfPinnedObject();

            // Setting up the rest of the parameters for EnumProcessModules
            uint uiSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * (hMods.Length));
            uint cbNeeded;
            if (NativeMethods.EnumProcessModulesEx(_processHandle, pModules, uiSize, out cbNeeded, DwFilterFlag.LIST_MODULES_ALL))
            {
                int modulesCount = (int)(cbNeeded / (Marshal.SizeOf(typeof(IntPtr))));
                for (int i = 0; i < modulesCount; i++)
                {
                    StringBuilder strbld = new StringBuilder(1024);
                    NativeMethods.GetModuleFileNameEx(_processHandle, hMods[i], strbld, (int)(strbld.Capacity));
                    string moduleName = strbld.ToString();

                    if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(moduleName, targetName, CompareOptions.IgnoreCase) >= 0)
                    {
                        result = hMods[i].ToInt64();
                        break;
                    }
                }
            }

            gch.Free();
            return result;
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
                throw new ProcessRamWatcherException("Could not read memory from a ProcessRamWatcher which failed to initialize");
            
            int bytesRead = 0;
            byte[] buffer = new byte[length];
            NativeMethods.ReadProcessMemory((int)_processHandle, address, buffer, length, ref bytesRead);
            if (bytesRead < length)
                throw new ProcessRamWatcherException("Could not read process memory at address 0x" + address.ToString("X"));

            // If data is meant to be read as big endian, reverse it for BitConverter methods to work as they should
            if (length > 1 && isBigEndian)
                Array.Reverse(buffer);

            return buffer;
        }

        public byte ReadUint8(long address) 
            => ReadBytes(address, sizeof(byte))[0];

        public ushort ReadUint16(long address) 
            => BitConverter.ToUInt16(ReadBytes(address, sizeof(ushort), IsBigEndian), 0);

        public uint ReadUint32(long address)
            => BitConverter.ToUInt32(ReadBytes(address, sizeof(uint), IsBigEndian), 0);

        public ulong ReadUint64(long address)
            => BitConverter.ToUInt64(ReadBytes(address, sizeof(ulong), IsBigEndian), 0);

        public sbyte ReadInt8(long address)
            => (sbyte)ReadBytes(address, sizeof(sbyte))[0];

        public short ReadInt16(long address)
            => BitConverter.ToInt16(ReadBytes(address, sizeof(short), IsBigEndian), 0);

        public int ReadInt32(long address)
            => BitConverter.ToInt32(ReadBytes(address, sizeof(int), IsBigEndian), 0);

        public long ReadInt64(long address)
            => BitConverter.ToInt64(ReadBytes(address, sizeof(long), IsBigEndian), 0);

        public float ReadFloat(long address)
            => BitConverter.ToSingle(ReadBytes(address, sizeof(float), IsBigEndian), 0);

        public double ReadDouble(long address)
            => BitConverter.ToDouble(ReadBytes(address, sizeof(long), IsBigEndian), 0);

        /// <summary>
        /// Resolve a pointer path starting from a base address, and following pointers while also applying the given offsets
        /// one by one. Such a path can be obtained with external programs like CheatEngine.
        /// </summary>
        /// <param name="baseAddress">the base address</param>
        /// <param name="offsets">the offsets to apply while reading a pointer for each one of them</param>
        /// <param name="is64Bit">a boolean telling if pointers are 64-bit long (true) or 32-bit long (false)</param>
        /// <returns>The address pointed by the pointer path, or 0 if the path is broken in any way.</returns>
        private long ResolvePointerPath(long baseAddress, int[] offsets, bool is64Bit)
        {
            if(!TestProcess())
                throw new ProcessRamWatcherException("Process was shutdown while connected");

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
            catch(ProcessRamWatcherException)
            {
                return 0;
            }
        }

        public long ResolvePointerPath32(long baseAddress, int[] offsets) => ResolvePointerPath(baseAddress, offsets, false);
        public long ResolvePointerPath64(long baseAddress, int[] offsets) => ResolvePointerPath(baseAddress, offsets, true);

        /// <summary>
        /// List all memory regions used by this process.
        /// </summary>
        /// <returns>A list of all memory regions having the properties specified in the input parameters</returns>
        public List<MemoryRegion> ListMemoryRegions()
        {
            List<MemoryRegion> regions = new List<MemoryRegion>();

            MEMORY_BASIC_INFORMATION info = new MEMORY_BASIC_INFORMATION();
            int mbiSize = Marshal.SizeOf(info);

            IntPtr ptr = IntPtr.Zero;
            while (NativeMethods.VirtualQueryEx(_processHandle, ptr, out info, (uint)mbiSize) == mbiSize)
            {
                ptr = (IntPtr)(ptr.ToInt64() + (long)info.RegionSize);

                regions.Add(new MemoryRegion()
                {
                    BaseAddress = (long)info.BaseAddress,
                    Size = (long)info.RegionSize,
                    Type = info.Type,
                });
            }

            return regions;
        }
        
        /// <summary>
        /// Test if the attached process memory is still accessible.
        /// </summary>
        /// <returns>true if memory can be read, false otherwise</returns>
        public bool TestProcess()
        {
            try
            {
                ReadUint8(BaseAddress);
                return true;
            }
            catch(ProcessRamWatcherException) 
            {
                return false;
            }
        }

        /// <summary>
        /// Perform a hash of the binary file the watcher is currently connected to, allowing to check
        /// the binary identity / version.
        /// </summary>
        /// <returns>A SHA-256 hash of the binary file</returns>
        public string GetBinaryHash()
        {
            uint size = 5096;
            StringBuilder stringBuilder = new StringBuilder((int)size);
            NativeMethods.QueryFullProcessImageName(_processHandle, 0, stringBuilder, ref size);
            string binaryFilePath = stringBuilder.ToString();

            using (var sha = SHA256.Create("System.Security.Cryptography.SHA256Cng"))
                using (var stream = File.OpenRead(binaryFilePath))
                    return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        /// <summary>
        /// Fetch the infamous THREADSTACK0 address (as specified by the CheatEngine software) to use as a base address
        /// for further data retrieval.
        /// </summary>
        private long GetThreadstack0Address()
        {
            string binaryName = Is64Bit ? "ThreadstackFinder64.exe" : "ThreadstackFinder32.exe";

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = binaryName,
                    Arguments = _process.Id.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
                return long.Parse(proc.StandardOutput.ReadLine());
            return 0;
        }
    }
}
