using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace HintMachine.Helpers
{
    internal class ThreadstackFinder
    {
        public static long FindThreadstack0(ProcessRamWatcher ram)
        {
            NativeMethods.IsWow64Process(ram.Process.Handle, out bool is32Bit);
            if (is32Bit)
                return FindThreadstack0_32(ram);
            return FindThreadstack0_64(ram);
        }

        private static long FindThreadstack0_64(ProcessRamWatcher ram)
        {
            uint firstTID = (uint)ram.Process.Threads.OfType<ProcessThread>().First().Id;
            IntPtr thread_handle = NativeMethods.OpenThread(ThreadAccess.GET_CONTEXT | ThreadAccess.QUERY_INFORMATION, false, firstTID);
            if (thread_handle == IntPtr.Zero)
                return 0;

            THREAD_BASIC_INFORMATION tbi = new THREAD_BASIC_INFORMATION();
            long stacktop = 0;
            int status = NativeMethods.NtQueryInformationThread(thread_handle, ThreadInfoClass.ThreadBasicInformation, out tbi, (uint)Marshal.SizeOf(tbi), IntPtr.Zero);
            if (status >= 0)
            {
                byte[] content = ram.ReadBytes((long)tbi.TebBaseAddress, (uint)Marshal.SizeOf(typeof(NT_TIB)));
                GCHandle PinnedStruct = GCHandle.Alloc(content, GCHandleType.Pinned);
                NT_TIB tib = (NT_TIB)Marshal.PtrToStructure(PinnedStruct.AddrOfPinnedObject(), typeof(NT_TIB));
                stacktop = tib.StackBase;
                PinnedStruct.Free();
            }

            NativeMethods.CloseHandle(thread_handle);
            if (stacktop == 0)
                return 0;

            IntPtr moduleHandle = NativeMethods.GetModuleHandle("kernel32.dll");
            if (moduleHandle == IntPtr.Zero)
                return 0;

            MODULEINFO mi = new MODULEINFO();
            NativeMethods.GetModuleInformation(ram.Process.Handle, moduleHandle, out mi, (uint)Marshal.SizeOf(mi));

            const int PTR_SIZE = 8;
            const int STACK_SIZE = 8192;

            ulong[] buf = new ulong[1024];
            byte[] bytes = ram.ReadBytes((long)stacktop - STACK_SIZE, STACK_SIZE);
            Buffer.BlockCopy(bytes, 0, buf, 0, bytes.Length);

            for (long i = STACK_SIZE / PTR_SIZE - 1; i >= 0; --i)
            {
                if (buf[i] >= mi.lpBaseOfDll && buf[i] <= mi.lpBaseOfDll + mi.SizeOfImage)
                {
                    return stacktop - STACK_SIZE + (i * PTR_SIZE);
                }
            }

            return 0;
        }

        private static long FindThreadstack0_32(ProcessRamWatcher ram)
        {
            uint firstTID = (uint)ram.Process.Threads.OfType<ProcessThread>().First().Id;
            IntPtr thread_handle = NativeMethods.OpenThread(ThreadAccess.GET_CONTEXT | ThreadAccess.QUERY_INFORMATION, false, firstTID);
            if (thread_handle == IntPtr.Zero)
                return 0;

            WOW64_CONTEXT context = new WOW64_CONTEXT();
            context.ContextFlags = 0x10004;

            bool result = NativeMethods.Wow64GetThreadContext(thread_handle, out context);
            if (!result)
                return 0;

            WOW64_LDT_ENTRY entry = new WOW64_LDT_ENTRY();
            if (!NativeMethods.Wow64GetThreadSelectorEntry(thread_handle, context.SegFs, out entry))
                return 0;
            long addr = (uint)(entry.BaseLow | (entry.BaseMid << 16) | (entry.BaseHi << 24));
            long stacktop = ram.ReadUint32(addr + 4);

            NativeMethods.CloseHandle(thread_handle);
            if (stacktop == 0)
                return 0;

            IntPtr moduleHandle = NativeMethods.GetModuleHandle("kernel32.dll");
            if (moduleHandle == IntPtr.Zero)
                return 0;

            MODULEINFO mi = new MODULEINFO();
            mi.lpBaseOfDll = (ulong)ram.FindModuleBaseAddress("kernel32.dll");
            mi.SizeOfImage = 0xF0000;

            const int PTR_SIZE = 4;
            const int STACK_SIZE = 4096;

            uint[] buf = new uint[1024];
            byte[] bytes = ram.ReadBytes((long)stacktop - STACK_SIZE, STACK_SIZE);
            Buffer.BlockCopy(bytes, 0, buf, 0, bytes.Length);

            for (int i = STACK_SIZE / PTR_SIZE - 1; i >= 0; --i)
            {
                if (buf[i] >= (ulong)mi.lpBaseOfDll && buf[i] <= (ulong)mi.lpBaseOfDll + mi.SizeOfImage)
                {
                    return stacktop - STACK_SIZE + (i * PTR_SIZE);
                }
            }

            return 0;
        }
    }
}
