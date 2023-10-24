#include <iostream>
#include <string>
#include <windows.h>
#include <TlHelp32.h>
#include <Psapi.h>

#if _WIN64
	typedef uint64_t StackPtrType;
#else
	typedef uint32_t StackPtrType;
#endif

constexpr int PTR_SIZE = sizeof(StackPtrType);
constexpr int STACK_SIZE = 1024 * PTR_SIZE;

struct CLIENT_ID
{
	PVOID UniqueProcess;
	PVOID UniqueThread;
};

struct THREAD_BASIC_INFORMATION
{
	NTSTATUS ExitStatus;
	PVOID TebBaseAddress;
	CLIENT_ID ClientId;
	KAFFINITY AffinityMask;
	DWORD Priority;
	DWORD BasePriority;
};

enum THREADINFOCLASS
{
	ThreadBasicInformation,
};

uint64_t find_thread_stack_top(HANDLE process_handle, HANDLE thread_handle)
{
	bool loadedManually = false;
	HMODULE module = GetModuleHandle("ntdll.dll");
	if (!module)
	{
		module = LoadLibrary("ntdll.dll");
		if (!module)
			return 0;

		loadedManually = true;
	}

	uint64_t result = 0;

	NTSTATUS(__stdcall * NtQueryInformationThread)(HANDLE ThreadHandle, THREADINFOCLASS ThreadInformationClass, PVOID ThreadInformation, ULONG ThreadInformationLength, PULONG ReturnLength);
	NtQueryInformationThread = reinterpret_cast<decltype(NtQueryInformationThread)>(GetProcAddress(module, "NtQueryInformationThread"));
	if (NtQueryInformationThread)
	{
		NT_TIB tib = { 0 };
		THREAD_BASIC_INFORMATION tbi = { 0 };

		NTSTATUS status = NtQueryInformationThread(thread_handle, ThreadBasicInformation, &tbi, sizeof(tbi), nullptr);
		if (status >= 0)
		{
			ReadProcessMemory(process_handle, tbi.TebBaseAddress, &tib, sizeof(tbi), nullptr);
			result = (uint64_t)tib.StackBase;
		}
	}

	if (loadedManually)
		FreeLibrary(module);
	return result;
}

uint64_t find_threadstack_for_thread(HANDLE process_handle, DWORD thread_id)
{
	HANDLE thread_handle = OpenThread(THREAD_GET_CONTEXT | THREAD_QUERY_INFORMATION, FALSE, thread_id);
	if (!thread_handle)
		return 0;

	uint64_t stacktop = find_thread_stack_top(process_handle, thread_handle);
	CloseHandle(thread_handle);

	if (!stacktop)
		return 0;

	MODULEINFO mi;
	HMODULE kernel_module = GetModuleHandle("kernel32.dll");
	if (!kernel_module)
		return 0;

	GetModuleInformation(process_handle, kernel_module, &mi, sizeof(mi));

	StackPtrType buf[1024] {};
	if (ReadProcessMemory(process_handle, (LPCVOID)(stacktop - STACK_SIZE), buf, STACK_SIZE, NULL))
	{
		for (int i = STACK_SIZE / PTR_SIZE - 1; i >= 0; --i)
		{
			if (buf[i] >= (uint64_t)mi.lpBaseOfDll && buf[i] <= (uint64_t)mi.lpBaseOfDll + mi.SizeOfImage)
			{
				return stacktop - STACK_SIZE + (i * PTR_SIZE);
				break;
			}
		}
	}

	return 0;
}

DWORD get_first_thread_id(DWORD pid)
{
	HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
	if (h == INVALID_HANDLE_VALUE)
		return -1;

	THREADENTRY32 te {};
	te.dwSize = sizeof(te);
	if (Thread32First(h, &te))
	{
		do {
			if (te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(te.th32OwnerProcessID))
			{
				if (te.th32OwnerProcessID == pid)
					return te.th32ThreadID;
			}
			te.dwSize = sizeof(te);
		} while (Thread32Next(h, &te));
	}

	return -1;
}

int main(int argc, char** argv)
{
	try
	{
		DWORD pid = std::stol(argv[1]);
		
		HANDLE process_handle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
		if (!process_handle || process_handle == INVALID_HANDLE_VALUE)
			return EXIT_FAILURE;

		DWORD thread_id = get_first_thread_id(pid);

		uint64_t threadstack_0 = find_threadstack_for_thread(process_handle, thread_id);
		std::cout << threadstack_0 << std::endl;
		return EXIT_SUCCESS;
	}
	catch (std::invalid_argument&)
	{
		return EXIT_FAILURE;
	}

}