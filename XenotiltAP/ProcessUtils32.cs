using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace XenotiltAP
{
    public static class ProcessUtils32
    {
        public static class CheatengineSpecific
        {
            /// <summary>
            /// Returns the address of the ThreadStack0 special symbol.
            /// </summary>
            /// <param name="process">The target process</param>
            public static uint GetThreadStack0(Process process)
            {
                if (!InfoReader.Is32BitProcess(process.Handle))
                    throw new ArgumentException("Provided process is not 32 bit.");

                const uint BytesToSample = 4096; // Arbitrary (?). 4096 should be enough for both x86 and x64; should be divisible by 4 (x86)
                MODULEINFO mi = InfoReader.GetKernel32ModuleInfo(process); // MODULEINFO delivers the Kernel32 module's load address and size (note that load address (lpBaseOfDll) is the same as the module handle)
                NT_TIB tib = InfoReader.GetTIB(process); // NT_TIB delivers the stack base address of the process' main thread

                // Read sample byte array from the base of the main thread stack
                byte[] StackBaseSample = MemReader.ReadToByteArray(process.Handle, (tib.StackBase - BytesToSample), BytesToSample);

                int i = 0; // Keep scope bigger than loop

                // ThreadStack0 is the first pointer in the main thread's stack that points inside the Kernel32 module.
                // To find it, we iterate through each 32 bit (4 byte) value in the stack sample, and check if it is in the target range.
                for (i = (StackBaseSample.Length / 4) - 1; i >= 0; --i)
                {
                    UInt32 valueAtPosition = BitConverter.ToUInt32(StackBaseSample, i * 4);
                    if (valueAtPosition >= (uint)mi.lpBaseOfDll &&
                        valueAtPosition <= (uint)mi.lpBaseOfDll + mi.SizeOfImage)
                        break;
                }

                if (i == 0) // If i reached zero, then iteration finished without finding a match
                    throw new Exceptions.LocalException(nameof(GetThreadStack0), "ThreadStack0 can't be found in the sampled " + BytesToSample + " bytes");

                // Finally, calculate and return the actual ThreadStack0 address from index i
                return (uint)(tib.StackBase - BytesToSample + i * 4);
            }

            public static uint GetThreadStack0_64(Process process)
            {
                if (!InfoReader.Is64BitProcess(process.Handle))
                    throw new ArgumentException("Provided process is not 64 bit.");

                const uint BytesToSample = 8192; // Arbitrary (?). 4096 should be enough for both x86 and x64; should be divisible by 4 (x86)
                MODULEINFO mi = InfoReader.GetKernel64ModuleInfo(process); // MODULEINFO delivers the Kernel32 module's load address and size (note that load address (lpBaseOfDll) is the same as the module handle)
                NT_TIB tib = InfoReader.GetTIB(process); // NT_TIB delivers the stack base address of the process' main thread

                // Read sample byte array from the base of the main thread stack
                byte[] StackBaseSample = MemReader.ReadToByteArray(process.Handle, (tib.StackBase - BytesToSample), BytesToSample);

                int i = 0; // Keep scope bigger than loop

                // ThreadStack0 is the first pointer in the main thread's stack that points inside the Kernel32 module.
                // To find it, we iterate through each 32 bit (4 byte) value in the stack sample, and check if it is in the target range.
                for (i = (StackBaseSample.Length / 4) - 1; i >= 0; --i)
                {
                    UInt32 valueAtPosition = BitConverter.ToUInt32(StackBaseSample, i * 4);
                    if (valueAtPosition >= (uint)mi.lpBaseOfDll &&
                        valueAtPosition <= (uint)mi.lpBaseOfDll + mi.SizeOfImage)
                        break;
                }

                if (i == 0) // If i reached zero, then iteration finished without finding a match
                    throw new Exceptions.LocalException(nameof(GetThreadStack0), "ThreadStack0 can't be found in the sampled " + BytesToSample + " bytes");

                // Finally, calculate and return the actual ThreadStack0 address from index i
                return (uint)(tib.StackBase - BytesToSample + i * 4);
            }

            /// <summary>
            /// Traces a pointer chain and returns the value found at the last pointer address.
            /// </summary>
            /// <param name="processHandle">The handle of the target process.</param>
            /// <param name="baseAddress">The address from which the pointer tracing should begin.</param>
            /// <param name="jumpsWithOffsets">Array that defines the jumps in the chain. Use 0 if offset is not needed.</param>
            public static T ReadPointerChain<T>(IntPtr processHandle, uint baseAddress, int[] jumpsWithOffsets)
            {
                if (!InfoReader.Is32BitProcess(processHandle))
                    throw new ArgumentException("Provided process is not 32 bit.");

                // Process pointer chain up to the last element (excluding the last)
                uint Value = baseAddress;
                for (int i = 0; i < jumpsWithOffsets.Length - 1; i++)
                    Value = MemReader.ReadToType<uint>(processHandle, (uint)(Value + jumpsWithOffsets[i]));

                // Read memory at last pointer address into to the requested type
                T t = MemReader.ReadToType<T>(processHandle, (uint)(Value + jumpsWithOffsets.Last()));

                return t;
            }

            /// <summary>
            /// Traces a pointer chain and returns the value found at the last pointer address.
            /// </summary>
            /// <param name="processHandle">The handle of the target process.</param>
            /// <param name="baseAddress">The address from which the pointer tracing should begin.</param>
            /// <param name="jumpsWithOffsets">Array that defines the jumps in the chain. Use 0 if offset is not needed.</param>
            public static T ReadPointerChain_64<T>(IntPtr processHandle, uint baseAddress, int[] jumpsWithOffsets)
            {
                if (InfoReader.Is32BitProcess(processHandle))
                    throw new ArgumentException("Provided process is not 64 bit.");

                // Process pointer chain up to the last element (excluding the last)
                uint Value = baseAddress;
                for (int i = 0; i < jumpsWithOffsets.Length - 1; i++)
                    Value = MemReader.ReadToType<uint>(processHandle, (uint)(Value + jumpsWithOffsets[i]));

                // Read memory at last pointer address into to the requested type
                T t = MemReader.ReadToType<T>(processHandle, (uint)(Value + jumpsWithOffsets.Last()));

                return t;
            }
        }

        // Helper class for reading other processes' private memory area with native windows function calls
        public static class MemReader
        {
            /// <summary>
            /// Reads target memory area into a type. The size of the area to be read is determined by the size of the type (i.e. int32 = 4 bytes). If you specify a struct as a type parameter, this method can fill the struct with the target memory area.
            /// </summary>
            /// <param name="processHandle">The handle of the target process</param>
            /// <param name="memoryAddress">The private memory address of the process to read from. Effectively a pointer.</param>
            /// <returns>Returns T filled with the target memory area</returns>
            public static T ReadToType<T>(IntPtr processHandle, uint memoryAddress)
            {
                // Fall back to byte array read, since our PInvoke can't be generic
                byte[] content = ReadToByteArray(processHandle, memoryAddress, (uint)Marshal.SizeOf(typeof(T)));

                // Fill our generic variable with the byte array
                T t;
                GCHandle PinnedStruct = GCHandle.Alloc(content, GCHandleType.Pinned);
                try { t = (T)Marshal.PtrToStructure(PinnedStruct.AddrOfPinnedObject(), typeof(T)); }
                catch (Exception ex) { throw new Exceptions.LocalException(nameof(ReadToType), "Error trying to fill the return type.", ex); }
                finally { PinnedStruct.Free(); }

                return t;
            }

            public static byte[] ReadToByteArray(IntPtr processHandle, uint memoryAddress, uint numberOfBytes)
            {
                byte[] bytes = new byte[numberOfBytes];
                var result = ReadProcessMemory(processHandle, memoryAddress, bytes, (uint)bytes.Length, IntPtr.Zero);

                // ReadProcessMemory returns 0 if failed
                if (!result)
                    throw new Exceptions.PInvokeException(nameof(ReadToByteArray), nameof(ReadProcessMemory), Marshal.GetLastWin32Error());

                return bytes;
            }

            #region PInvoke Declarations
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ReadProcessMemory(
                IntPtr hProcess,
                uint lpBaseAddress,
                byte[] lpBuffer,
                uint nSize,
                IntPtr lpNumberOfBytesRead);
            #endregion
        }

        // Helper class for reading other processes' process information with native windows function calls
        public static class InfoReader
        {
            /// <summary>
            /// Checks if process handle is associated to a 32 bit process.
            /// </summary>
            /// <param name="processHandle">The handle of the process to check.</param>
            /// <returns>Returns true if process is 32 bit, false if it is 64bit.</returns>
            public static bool Is32BitProcess(IntPtr processHandle)
            {
                bool Is32Bit = false;
                try { IsWow64Process(processHandle, out Is32Bit); }
                catch { throw new Exceptions.PInvokeException(nameof(Is32BitProcess), nameof(IsWow64Process), Marshal.GetLastWin32Error()); }

                return Is32Bit;
            }

            public static bool Is64BitProcess(IntPtr processHandle)
            {
                bool Is64Bit = false;
                try { IsWow64Process(processHandle, out Is64Bit); }
                catch { throw new Exceptions.PInvokeException(nameof(Is64BitProcess), nameof(IsWow64Process), Marshal.GetLastWin32Error()); }

                return Is64Bit;
            }

            public static THREAD_BASIC_INFORMATION GetTBI(Process process)
            {
                IntPtr hThread = OpenThread(ThreadAccess.QueryInformation, false, (uint)process.Threads.OfType<ProcessThread>().First().Id);
                if (hThread == null) // Some implementations on StackExchange compare against IntPtr.zero - but MSDN says it returns null, not null pointer
                    throw new Exceptions.PInvokeException(nameof(GetTBI), nameof(OpenThread), Marshal.GetLastWin32Error());

                try
                {
                    THREAD_BASIC_INFORMATION tbi = new THREAD_BASIC_INFORMATION();
                    int result = NtQueryInformationThread(hThread, ThreadInfoClass.ThreadBasicInformation, out tbi, (uint)Marshal.SizeOf(tbi), IntPtr.Zero);
                    // NtQueryInformationThread returns nonzero if failed
                    if (result != 0)
                        throw new Exceptions.PInvokeException(nameof(GetTBI), nameof(NtQueryInformationThread), Marshal.GetLastWin32Error());

                    return tbi;
                }
                finally
                {
                    CloseHandle(hThread);
                }
            }

            public static THREAD_BASIC_INFORMATION GetTBI_64(Process process)
            {
                IntPtr hThread = OpenThread(ThreadAccess.QueryInformation, false, (uint)process.Threads.OfType<ProcessThread>().First().Id);
                if (hThread == null) // Some implementations on StackExchange compare against IntPtr.zero - but MSDN says it returns null, not null pointer
                    throw new Exceptions.PInvokeException(nameof(GetTBI), nameof(OpenThread), Marshal.GetLastWin32Error());

                try
                {
                    THREAD_BASIC_INFORMATION tbi = new THREAD_BASIC_INFORMATION();
                    int result = NtQueryInformationThread(hThread, ThreadInfoClass.ThreadBasicInformation, out tbi, (uint)Marshal.SizeOf(tbi), IntPtr.Zero);
                    // NtQueryInformationThread returns nonzero if failed
                    if (result != 0)
                        throw new Exceptions.PInvokeException(nameof(GetTBI), nameof(NtQueryInformationThread), Marshal.GetLastWin32Error());

                    return tbi;
                }
                finally
                {
                    CloseHandle(hThread);
                }
            }

            public static NT_TIB GetTIB(Process process)
            {
                // Read TBI - prerequise for TIB
                THREAD_BASIC_INFORMATION tbi = GetTBI(process);

                // Read NT_TIB equivalent memory area of the process directly into our NT_TIB struct
                NT_TIB tib = MemReader.ReadToType<NT_TIB>(process.Handle, tbi.TebBaseAddress);

                return tib;
            }

            public static MODULEINFO GetKernel32ModuleInfo(Process process)
            {
                // GetModuleHandle returns NULL when failed
                IntPtr moduleHandle = GetModuleHandle("kernel32.dll");
                if (moduleHandle == null)
                    throw new Exceptions.PInvokeException(nameof(GetKernel32ModuleInfo), nameof(GetModuleHandle), Marshal.GetLastWin32Error());

                MODULEINFO mi = new MODULEINFO();

                // GetModuleInformation returns 0 when failed
                var result = GetModuleInformation(process.Handle, moduleHandle, out mi, (uint)Marshal.SizeOf(mi));
                if (!result)
                    throw new Exceptions.PInvokeException(nameof(GetKernel32ModuleInfo), nameof(GetModuleInformation), Marshal.GetLastWin32Error());

                return mi;
            }

            public static MODULEINFO GetKernel64ModuleInfo(Process process)
            {
                // GetModuleHandle returns NULL when failed
                IntPtr moduleHandle = GetModuleHandle("kernel32.dll");
                if (moduleHandle == null)
                    throw new Exceptions.PInvokeException(nameof(GetKernel64ModuleInfo), nameof(GetModuleHandle), Marshal.GetLastWin32Error());

                MODULEINFO mi = new MODULEINFO();

                // GetModuleInformation returns 0 when failed
                var result = GetModuleInformation(process.Handle, moduleHandle, out mi, (uint)Marshal.SizeOf(mi));
                if (!result)
                    throw new Exceptions.PInvokeException(nameof(GetKernel64ModuleInfo), nameof(GetModuleInformation), Marshal.GetLastWin32Error());

                return mi;
            }

            #region PInvoke Declarations
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr hObject);

            [DllImport("psapi.dll", SetLastError = true)]
            private static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("ntdll.dll", SetLastError = true, CallingConvention =CallingConvention.StdCall)]
            private static extern int NtQueryInformationThread(
                IntPtr threadHandle,
                ThreadInfoClass threadInformationClass,
                out THREAD_BASIC_INFORMATION threadInformation,
                ulong threadInformationLength,
                IntPtr returnLengthPtr);

            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool IsWow64Process(
                [In] IntPtr processHandle,
                [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);
            #endregion

            #region PInvoke Data Structures
            [StructLayout(LayoutKind.Sequential)]
            public struct THREAD_BASIC_INFORMATION
            {
                public uint ExitStatus; // original: LONG NTSTATUS
                public uint TebBaseAddress; // original: PVOID
                public CLIENT_ID ClientId;
                public uint AffinityMask; // original: ULONG_PTR
                public uint Priority; // original: DWORD
                public uint BasePriority; // original: DWORD
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CLIENT_ID
            {
                public uint UniqueProcess; // original: PVOID
                public uint UniqueThread; // original: PVOID
            }

            [Flags]
            private enum ThreadAccess : int
            {
                Terminate = 0x0001,
                SuspendResume = 0x0002,
                GetContext = 0x0008,
                SetContext = 0x0010,
                SetInformation = 0x0020,
                QueryInformation = 0x0040,
                SetThreadToken = 0x0080,
                Impersonate = 0x0100,
                DirectImpersonation = 0x0200
            }

            private enum ThreadInfoClass : int
            {
                ThreadBasicInformation = 0,
                ThreadQuerySetWin32StartAddress = 9
            }
            #endregion
        }

        #region PInvoke Data Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct NT_TIB
        {
            public uint ExceptionListPointer; // Current Structured Exception Handling (SEH) frame
            public uint StackBase; // Bottom of stack (high address)
            public uint StackLimit; // Ceiling of stack (low address)
            public uint SubSystemTib; // No clue...
                                      // Incomplete representation of winnt.h / NT_TIB, but we're reading it directly from memory, so struct size doesn't matter
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public uint lpBaseOfDll;
            public uint SizeOfImage;
            public uint EntryPoint;
        }
        #endregion

        public class Exceptions
        {

            [Serializable()]
            public class PInvokeException : Exception, ISerializable
            {
                public string LocalMethod { get; private set; }
                public string PInvokeMethod { get; private set; }
                public int ErrorCode { get; private set; }

                private static Func<string, string, int, string> PInvokeErrorMsg = (LocalMethod, PInvokeMethod, ErrorCode) => String.Format("PInvoke '{0}' failed in method '{1}'. ErrorCode: {2}, Message: {3}", PInvokeMethod, LocalMethod, ErrorCode, new Win32Exception(ErrorCode).Message);

                public PInvokeException(string localMethod, string pInvokeMethod, int errorCode) : base(PInvokeErrorMsg(localMethod, pInvokeMethod, errorCode))
                {
                    LocalMethod = localMethod;
                    PInvokeMethod = pInvokeMethod;
                    ErrorCode = errorCode;
                }
                public PInvokeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
            }

            [Serializable()]
            public class LocalException : Exception, ISerializable
            {
                public string LocalMethod { get; private set; }
                public string Reason { get; private set; }

                private static Func<string, string, string> LocalErrorMsg = (LocalMethod, Reason) => String.Format("Method '{0}' failed with error: {1}", LocalMethod, Reason);

                public LocalException(string localMethod, string reason) : base(LocalErrorMsg(localMethod, reason))
                {
                    LocalMethod = localMethod;
                    Reason = reason;
                }
                public LocalException(string localMethod, string reason, Exception innerException) : base(LocalErrorMsg(localMethod, reason), innerException)
                {
                    LocalMethod = localMethod;
                    Reason = reason;
                }
                public LocalException(SerializationInfo info, StreamingContext context) : base(info, context) { }
            }
        }
    }
}
