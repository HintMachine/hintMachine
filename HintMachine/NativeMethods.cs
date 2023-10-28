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
        public static extern Microsoft.Win32.SafeHandles.SafeAccessTokenHandle OpenThread(
            ThreadAccess dwDesiredAccess,
            bool bInheritHandle,
            uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            int hProcess,
            long lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesRead);

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
    }
}
