using System;
using System.Runtime.InteropServices;
using System.Text;
using HintMachine.Helpers;

namespace HintMachine
{
    [Flags]
    public enum ThreadAccess : int
    {
        TERMINATE = 0x0001,
        SUSPEND_RESUME = 0x0002,
        GET_CONTEXT = 0x0008,
        SET_CONTEXT = 0x0010,
        SET_INFORMATION = 0x0020,
        QUERY_INFORMATION = 0x0040,
        SET_THREAD_TOKEN = 0x0080,
        IMPERSONATE = 0x0100,
        DIRECT_IMPERSONATION = 0x0200
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public MemoryRegionType Type;
    }

    public enum DwFilterFlag : uint
    {
        LIST_MODULES_DEFAULT = 0x0,
        LIST_MODULES_32BIT = 0x01,
        LIST_MODULES_64BIT = 0x02,
        LIST_MODULES_ALL = LIST_MODULES_32BIT | LIST_MODULES_64BIT
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct THREAD_BASIC_INFORMATION
    {
        public ulong ExitStatus;
        public ulong TebBaseAddress;
        public ulong ClientId;
        public ulong AffinityMask;
        public ulong Priority;
        public ulong BasePriority;
    }

    public enum ThreadInfoClass : int
    {
        ThreadBasicInformation = 0,
        ThreadQuerySetWin32StartAddress = 9
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NT_TIB
    {
        public ulong ExceptionListPointer;
        public long StackBase;
        public ulong StackLimit;
        public ulong SubSystemTib;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MODULEINFO
    {
        public ulong lpBaseOfDll;
        public uint SizeOfImage;
        public ulong EntryPoint;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WOW64_FLOATING_SAVE_AREA
    {
        public uint ControlWord;
        public uint StatusWord;
        public uint TagWord;
        public uint ErrorOffset;
        public uint ErrorSelector;
        public uint DataOffset;
        public uint DataSelector;
        public unsafe fixed byte RegisterArea[80];
        public uint Cr0NpxState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WOW64_CONTEXT
    {
        public uint ContextFlags;
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        public WOW64_FLOATING_SAVE_AREA FloatSave;
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        public unsafe fixed byte ExtendedRegisters[512];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WOW64_LDT_ENTRY
    {
        public ushort LimitLow;
        public ushort BaseLow;
        public byte BaseMid;
        public byte Flags1;
        public byte Flags2;
        public byte BaseHi;
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(
            IntPtr hHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(
            ThreadAccess dwDesiredAccess,
            bool bInheritHandle,
            uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            int hProcess,
            long lpBaseAddress,
            byte[] lpBuffer,
            uint dwSize,
            ref uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            uint dwLength);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModulesEx(
            IntPtr hProcess,
            [Out] IntPtr lphModule,
            uint cb,
            [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded,
            DwFilterFlag dwff);

        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint GetModuleFileNameEx(
             IntPtr hProcess,
             IntPtr hModule,
             [Out] StringBuilder lpBaseName,
             [In][MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool QueryFullProcessImageName(
            IntPtr hProcess,
            uint dwFlags,
            [Out] StringBuilder lpExeName,
            ref uint lpdwSize);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern int NtQueryInformationThread(
            IntPtr threadHandle,
            ThreadInfoClass threadInformationClass,
            out THREAD_BASIC_INFORMATION threadInformation,
            ulong threadInformationLength,
            IntPtr returnLengthPtr);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetModuleInformation(
            IntPtr hProcess,
            IntPtr hModule,
            out MODULEINFO lpmodinfo,
            uint cb);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(
            string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64GetThreadContext(
          IntPtr hThread,
          out WOW64_CONTEXT lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64GetThreadSelectorEntry(
          IntPtr hThread,
          uint dwSelector,
          out WOW64_LDT_ENTRY lpSelectorEntry);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool IsWow64Process(
            IntPtr hProcess,
            out bool Wow64Process);
    }
}
