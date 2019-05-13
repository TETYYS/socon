#include <windows.h>
#include <winternl.h>
#include <ntstatus.h>
#include <sddl.h>
#include <time.h>
#include <malloc.h>
#include "MarshalStruct.h"
#include "nt.h"

NTSYSAPI
PULONG
NTAPI
RtlSubAuthoritySid(
	_In_ PSID Sid,
	_In_ ULONG SubAuthority
);

NTSYSCALLAPI
NTSTATUS
NTAPI
NtQueryInformationToken(
	_In_ HANDLE TokenHandle,
	_In_ TOKEN_INFORMATION_CLASS TokenInformationClass,
	_Out_writes_bytes_(TokenInformationLength) PVOID TokenInformation,
	_In_ ULONG TokenInformationLength,
	_Out_ PULONG ReturnLength
);

NTSTATUS WINAPI RtlUnicodeToUTF8N(
	_Out_     PCHAR  UTF8StringDestination,
	_In_      ULONG  UTF8StringMaxByteCount,
	_Out_opt_ PULONG UTF8StringActualByteCount,
	_In_      PCWSTR UnicodeStringSource,
	_In_      ULONG  UnicodeStringWCharCount
);

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call) {
		case DLL_PROCESS_ATTACH:
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			break;
	}

	srand((UINT)time(NULL));
	return TRUE;
}

extern __declspec(dllexport) VOID FreeProcessImageName(PSTR ImageName)
{
	free(ImageName);
}

extern __declspec(dllexport) PSTR GetProcessImageName(HANDLE hProcess)
{
	NTSTATUS status;
	PUNICODE_STRING imageName;
	ULONG nameLen = 0;

	status = NtQueryInformationProcess(
		hProcess,
		43/*ProcessImageFileNameWin32*/,
		NULL,
		0,
		&nameLen
	);

	if (status != STATUS_BUFFER_OVERFLOW &&
		status != STATUS_BUFFER_TOO_SMALL &&
		status != STATUS_INFO_LENGTH_MISMATCH)
		return NULL;

	imageName = malloc(nameLen);
	status = NtQueryInformationProcess(
		hProcess,
		43/*ProcessImageFileNameWin32*/,
		imageName,
		nameLen,
		&nameLen
	);

	if (!NT_SUCCESS(status)) {
		free(imageName);
		return NULL;
	}

	ULONG utf8Bytes = 0;
	status = RtlUnicodeToUTF8N(
		NULL,
		0,
		&utf8Bytes,
		imageName->Buffer,
		imageName->Length / sizeof(*imageName->Buffer)
	);

	if (!NT_SUCCESS(status)) {
		free(imageName);
		return NULL;
	}

	PSTR utf8 = malloc(utf8Bytes);
	status = RtlUnicodeToUTF8N(
		utf8,
		utf8Bytes,
		&utf8Bytes,
		imageName->Buffer,
		imageName->Length / sizeof(*imageName->Buffer)
	);

	free(imageName);

	if (!NT_SUCCESS(status)) {
		free(imageName);
		return NULL;
	}

	return utf8;
}

extern __declspec(dllexport) CHAR GetProcessIntegrity(DWORD ProcessId)
{
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, FALSE, ProcessId);

	if (hProcess == NULL)
		return -1;
	
	HANDLE hToken;

	if (!OpenProcessToken(hProcess, TOKEN_QUERY, &hToken)) {
		CloseHandle(hProcess);
		return -2;
	}
	
	ULONG infoLen;

	NtQueryInformationToken(
		hToken,
		TokenIntegrityLevel,
		NULL,
		0,
		&infoLen
	);
	PTOKEN_MANDATORY_LABEL mandatoryLabel = malloc(infoLen);
	NTSTATUS status = NtQueryInformationToken(
		hToken,
		TokenIntegrityLevel,
		mandatoryLabel,
		infoLen,
		&infoLen
	);

	CloseHandle(hToken);
	CloseHandle(hProcess);

	if (!NT_SUCCESS(status)) {
		free(mandatoryLabel);
		return -3;
	}

	ULONG subAuthority = *RtlSubAuthoritySid(mandatoryLabel->Label.Sid, 0);
	free(mandatoryLabel);

	switch (subAuthority) {
		case SECURITY_MANDATORY_UNTRUSTED_RID:
			return 0;
		case SECURITY_MANDATORY_LOW_RID:
			return 1;
		case SECURITY_MANDATORY_MEDIUM_RID:
			return 2;
		case SECURITY_MANDATORY_MEDIUM_PLUS_RID:
			return 3;
		case SECURITY_MANDATORY_HIGH_RID:
			return 4;
		case SECURITY_MANDATORY_SYSTEM_RID:
			return 5;
		case SECURITY_MANDATORY_PROTECTED_PROCESS_RID:
			return 6;
		default:
			return -4;
	}
}

PCWSTR AllPrivs[] = {
	SE_CREATE_TOKEN_NAME,			SE_ASSIGNPRIMARYTOKEN_NAME,	SE_LOCK_MEMORY_NAME,			SE_INCREASE_QUOTA_NAME,
	SE_UNSOLICITED_INPUT_NAME,		SE_MACHINE_ACCOUNT_NAME,	SE_TCB_NAME,					SE_SECURITY_NAME,
	SE_TAKE_OWNERSHIP_NAME,			SE_LOAD_DRIVER_NAME,		SE_SYSTEM_PROFILE_NAME,			SE_SYSTEMTIME_NAME,
	SE_PROF_SINGLE_PROCESS_NAME,	SE_INC_BASE_PRIORITY_NAME,	SE_CREATE_PAGEFILE_NAME,		SE_CREATE_PERMANENT_NAME,
	SE_BACKUP_NAME,					SE_RESTORE_NAME,			SE_SHUTDOWN_NAME,				SE_DEBUG_NAME,
	SE_AUDIT_NAME,					SE_SYSTEM_ENVIRONMENT_NAME,	SE_CHANGE_NOTIFY_NAME,			SE_REMOTE_SHUTDOWN_NAME,
	SE_UNDOCK_NAME,					SE_SYNC_AGENT_NAME,			SE_ENABLE_DELEGATION_NAME,		SE_MANAGE_VOLUME_NAME,
	SE_IMPERSONATE_NAME,			SE_CREATE_GLOBAL_NAME,		SE_TRUSTED_CREDMAN_ACCESS_NAME,	SE_RELABEL_NAME,
	SE_INC_WORKING_SET_NAME,		SE_TIME_ZONE_NAME,			SE_CREATE_SYMBOLIC_LINK_NAME,	SE_DELEGATE_SESSION_USER_IMPERSONATE_NAME
};

extern __declspec(dllexport) BOOL EnablePrivileges()
{
	HANDLE hToken;
	LUID luid;
	PTOKEN_PRIVILEGES tokenPrivs = (PTOKEN_PRIVILEGES)alloca(sizeof(tokenPrivs->PrivilegeCount) + (sizeof(*tokenPrivs->Privileges) * ARRAYSIZE(AllPrivs)));

	if (tokenPrivs == NULL)
		return FALSE;

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken))
		return FALSE;

	tokenPrivs->PrivilegeCount = ARRAYSIZE(AllPrivs);
	for (SIZE_T x = 0; x < ARRAYSIZE(AllPrivs); x++) {
		if (!LookupPrivilegeValue(NULL, AllPrivs[x], &luid))
			goto lTokenFree;

		tokenPrivs->Privileges[x].Luid = luid;
		tokenPrivs->Privileges[x].Attributes = SE_PRIVILEGE_ENABLED;
	}

	if (!AdjustTokenPrivileges(hToken, FALSE, tokenPrivs, 0, NULL, NULL))
		goto lTokenFree;

	CloseHandle(hToken);
	return TRUE;
lTokenFree:
	CloseHandle(hToken);
	return FALSE;
}

extern __declspec(dllexport) HDESK ScwntCreateDesktop()
{
	WCHAR *desk = L"socon_";
	//for (DWORD x = 0; x < ARRAYSIZE(desk) - 6; x++)
		//desk[6 + x] = (WCHAR)(1 << (rand() % sizeof(WCHAR) * 8));
	
	SECURITY_ATTRIBUTES sa;
	
	sa.nLength = sizeof(sa);
	sa.bInheritHandle = TRUE;
	
	ConvertStringSecurityDescriptorToSecurityDescriptor(L"O:SYD:", SDDL_REVISION_1, &(sa.lpSecurityDescriptor), NULL);

	HDESK ret = CreateDesktopW(desk, NULL, NULL, 0, GENERIC_ALL, NULL/*&sa*/);
	LocalFree(sa.lpSecurityDescriptor);
	return ret;
}