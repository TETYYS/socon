#include <windows.h>

/*BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}*/

HHOOK CBTHook;
HHOOK GetMessageHook;
HINSTANCE hInst = NULL;

BOOL APIENTRY DllMain(HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	if (hInst != NULL) {
		//OutputDebugStringW(L"THE HOOK SAYS FUCK OFF");
		return FALSE;
	} else {
		//OutputDebugStringW(L"THE HOOK IS HERE");
		hInst = (HINSTANCE)hModule;
		return TRUE;
	}
}

LRESULT CALLBACK SysMsgProc(
	int code,  // hook code
	WPARAM wParam,  // removal flag
	LPARAM lParam  // address of structure with message
)
{
	if (code == HCBT_ACTIVATE)
	{
		WCHAR windtext[255];
		HWND Wnd = ((PMSG)lParam)->hwnd;
		GetWindowTextW(Wnd, windtext, 255);
		OutputDebugStringW(L"HCBT_ACTIVATE");
		// Here you can save active window title
	}

	if (code == HCBT_MINMAX)
	{
		WCHAR windtext[255];
		HWND Wnd = ((PMSG)lParam)->hwnd;
		GetWindowTextW(Wnd, windtext, 255);
		OutputDebugStringW(L"HCBT_MINMAX");
		// Here you can save active window title
	}

	if (code == HCBT_SETFOCUS)
	{
		WCHAR windtext[255];
		HWND Wnd = ((PMSG)lParam)->hwnd;
		GetWindowTextW(Wnd, windtext, 255);
		OutputDebugStringW(L"HCBT_SYSCOMMAND");
		// Here you can save active window title
	}

	if (code == HCBT_SETFOCUS)
	{
		WCHAR windtext[255];
		HWND Wnd = ((PMSG)lParam)->hwnd;
		GetWindowTextW(Wnd, windtext, 255);
		OutputDebugStringW(L"HCBT_SETFOCUS");
		// Here you can save active window title
	}

	if (code == HCBT_CREATEWND)
	{
		WCHAR windtext[255];
		HWND Wnd = ((PMSG)lParam)->hwnd;
		GetWindowTextW(Wnd, windtext, 255);
		OutputDebugStringW(L"HCBT_CREATEWND");
		// Here you can save New file title
	}
	return CallNextHookEx(CBTHook, code, wParam, lParam);
}

#pragma data_seg(".SHR")
DWORD ThePid = 0;
BOOL BoxEnabled = FALSE;
#pragma data_seg()
#pragma comment(linker, "/section:.SHR,rws")

DWORD *pids = NULL;
DWORD pidsCount = 0;

LRESULT CALLBACK GetMsgProc(
	_In_ int    nCode,
	_In_ WPARAM wParam,
	_In_ LPARAM lParam
)
{
	WCHAR f[256] = { 0 };

	if (!BoxEnabled)
		return CallNextHookEx(GetMessageHook, nCode, wParam, lParam);

	DWORD curProc = GetCurrentProcessId();
	for (DWORD x = 0; x < pidsCount; x++) {
		if (pids[x] == curProc)
			return FALSE; // Already in list
	}

	if (curProc == ThePid)
		return CallNextHookEx(GetMessageHook, nCode, wParam, lParam);

	pids = realloc(pids, ++pidsCount);
	pids[pidsCount - 1] = curProc;

	HANDLE hFile;
	BOOL flg;
	DWORD dwWrite;
	hFile = CreateFile(L"\\\\.\\pipe\\hkpipe", GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);

	if (hFile == INVALID_HANDLE_VALUE)
	{
		wsprintfW(f, L"CreateFile failed for Named Pipe client %d", GetLastError());
		OutputDebugStringW(f);
	}
	else
	{
		DWORD pid = GetCurrentProcessId();
		flg = WriteFile(hFile, &pid, sizeof(DWORD), &dwWrite, NULL);
		if (FALSE == flg)
			OutputDebugStringW(L"WriteFile failed for Named Pipe client\n");
		else
			OutputDebugStringW(L"WriteFile succeeded for Named Pipe client\n");

		CloseHandle(hFile);
	}
	return FALSE; // shoo
}

extern __declspec(dllexport) VOID TheBox(BOOL State, DWORD TheCode)
{
	if (TheCode != 0x9C04180B) {
		return 0;
	}

	BoxEnabled = State;
}

extern __declspec(dllexport) BOOL RunStopHook(BOOL State, DWORD TheCode)
{
	if (TheCode != 0x133F062A) {
		return FALSE;
	}

	if (State) {
		ThePid = GetCurrentProcessId();

		WCHAR f[256] = { 0 };
		wsprintfW(f, L"ThePid: %d", ThePid);
		OutputDebugStringW(f);
		CBTHook = SetWindowsHookExW(WH_CBT, &GetMsgProc, hInst, 0);
		OutputDebugStringW(L"THE HOOK");
		if (CBTHook != NULL)
			OutputDebugStringW(L"YES THE HOOK");
		else
			return FALSE;

		pidsCount++;
		pids = malloc(sizeof(DWORD) * pidsCount);

		GetMessageHook = SetWindowsHookExW(WH_GETMESSAGE, &GetMsgProc, hInst, 0);
		OutputDebugStringA("THE HOOKKK");
		if (GetMessageHook != NULL)
			OutputDebugStringA("YES THE HOOKKK");
		else
			return FALSE;

		return TRUE;
	} else {
		BOOL ret = TRUE;
		if (!UnhookWindowsHookEx(CBTHook))
			ret = FALSE;
		if (!UnhookWindowsHookEx(GetMessageHook))
			ret = FALSE;

		if (!ret)
			return FALSE;

		free(pids);
		pidsCount = 0;

		return TRUE;
	}
}