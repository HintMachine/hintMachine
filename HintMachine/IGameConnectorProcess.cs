using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HintMachine
{
    public abstract class IGameConnectorProcess : IGameConnector
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

        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationThread(IntPtr processHandle, int threadInformationClass, IntPtr threadInformation, uint threadInformationLength, IntPtr returnLength);

        // ----------------------------------------------------------------------------------

        public string processName;
        public string moduleName;
        public Process process = null;
        public ProcessModule module = null;
        public IntPtr processHandle = IntPtr.Zero;

        public IGameConnectorProcess(string processName, string moduleName = "")
        {
            this.processName = processName;
            this.moduleName = moduleName;
        }

        public override bool Connect()
        {
            TokenManipulator.AddPrivilege("SeDebugPrivilege");
            TokenManipulator.AddPrivilege("SeSystemEnvironmentPrivilege");
            Process.EnterDebugMode();

            Process[] processes = Process.GetProcessesByName(processName);
            if(processes.Length == 0)
                return false;
            process = processes[0];

            if(moduleName == "")
            {
                module = process.MainModule;
            }
            else
            {
                foreach (ProcessModule m in process.Modules)
                {
                    if (m.FileName.Equals(moduleName))
                    {
                        module = m;
                        break;
                    }
                }
            }

            processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_QUERY_INFORMATION, false, process.Id);
            
            return process != null && module != null && processHandle != IntPtr.Zero;
        }

        public override void Disconnect()
        {
            process = null;
            module = null;
            processHandle = IntPtr.Zero;
        }

        protected long ResolvePointerPath(long baseAddress, int[] offsets)
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

        protected long ResolvePointerPath32(long baseAddress, int[] offsets)
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


        protected byte[] ReadBytes(long address, int length, bool isBigEndian = false)
        {
            if (processHandle == IntPtr.Zero)
            {
                byte[] zeroByteArray = new byte[length];
                for(int i = 0; i<length; ++i)
                    zeroByteArray[i] = 0;
                return zeroByteArray;
            }

            int bytesRead = 0;
            byte[] buffer = new byte[length];
            ReadProcessMemory((int)processHandle, address, buffer, length, ref bytesRead);

            // If data is meant to be read as big endian, reverse it for BitConverter methods to work as they should
            if (length > 1 && isBigEndian)
                Array.Reverse(buffer);

            return buffer;
        }

        protected byte ReadUint8(long address)
        {
            return ReadBytes(address, sizeof(byte))[0];
        }

        protected ushort ReadUint16(long address, bool isBigEndian = false)
        {
            return BitConverter.ToUInt16(ReadBytes(address, sizeof(ushort), isBigEndian), 0);
        }

        protected uint ReadUint32(long address, bool isBigEndian = false)
        {
            return BitConverter.ToUInt32(ReadBytes(address, sizeof(uint), isBigEndian), 0);
        }

        protected ulong ReadUint64(long address, bool isBigEndian = false)
        {
            return BitConverter.ToUInt64(ReadBytes(address, sizeof(ulong), isBigEndian), 0);
        }

        protected sbyte ReadInt8(long address)
        {
            return (sbyte)ReadBytes(address, sizeof(sbyte))[0];
        }

        protected short ReadInt16(long address, bool isBigEndian = false)
        {
            return BitConverter.ToInt16(ReadBytes(address, sizeof(short), isBigEndian), 0);
        }

        protected int ReadInt32(long address, bool isBigEndian = false)
        {
            return BitConverter.ToInt32(ReadBytes(address, sizeof(int), isBigEndian), 0);
        }

        protected long ReadInt64(long address, bool isBigEndian = false)
        {
            return BitConverter.ToInt64(ReadBytes(address, sizeof(long), isBigEndian), 0);
        }
    }
}
