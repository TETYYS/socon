using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace socon.Native
{
	static class WinAPI
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr LoadLibrary(string libname);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[Flags]
		public enum ThreadAccess : uint
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

		[Flags]
		public enum ProcessAccess : uint
		{
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VirtualMemoryOperation = 0x00000008,
			VirtualMemoryRead = 0x00000010,
			VirtualMemoryWrite = 0x00000020,
			DuplicateHandle = 0x00000040,
			CreateProcess = 0x000000080,
			SetQuota = 0x00000100,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			QueryLimitedInformation = 0x00001000,
			Synchronize = 0x00100000
		}

		[Flags]
		public enum DESKTOP_ACCESS : uint {
			DESKTOP_NONE = 0,
			DESKTOP_READOBJECTS = 0x0001,
			DESKTOP_CREATEWINDOW = 0x0002,
			DESKTOP_CREATEMENU = 0x0004,
			DESKTOP_HOOKCONTROL = 0x0008,
			DESKTOP_JOURNALRECORD = 0x0010,
			DESKTOP_JOURNALPLAYBACK = 0x0020,
			DESKTOP_ENUMERATE = 0x0040,
			DESKTOP_WRITEOBJECTS = 0x0080,
			DESKTOP_SWITCHDESKTOP = 0x0100,

			GENERIC_ALL = (	DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW | DESKTOP_CREATEMENU |
							DESKTOP_HOOKCONTROL | DESKTOP_JOURNALRECORD | DESKTOP_JOURNALPLAYBACK |
							DESKTOP_ENUMERATE | DESKTOP_WRITEOBJECTS | DESKTOP_SWITCHDESKTOP),
		}

		[Flags]
		public enum EVENT_ACCESS : uint {
			EVENT_MODIFY_STATE = 0x0002, // Modify state access, which is required for the SetEvent, ResetEvent and PulseEvent functions.
			DELETE = 0x00010000, // Required to delete the object.
			EVENT_ALL_ACCESS = 0x1F0003, // All possible access rights for an event object. Use this right only if your application requires access beyond that granted by the standard access rights and EVENT_MODIFY_STATE. Using this access right increases the possibility that your application must be run by an Administrator.
			READ_CONTROL = 0x00020000, // Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
			SYNCHRONIZE = 0x00100000, // The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
			WRITE_DAC = 0x00040000, // Required to modify the DACL in the security descriptor for the object.
			WRITE_OWNER = 0x00080000 // Required to change the owner in the security descriptor for the object.
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint ResumeThread(IntPtr hThread);

		[DllImport("ntdll.dll", SetLastError = true)]
		public static extern int NtResumeProcess(IntPtr ProcessHandle);

		[DllImport("ntdll.dll", SetLastError = true)]
		public static extern int NtSuspendProcess(IntPtr ProcessHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccess processAccess, bool bInheritHandle, uint processId);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetDC(IntPtr hwnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern void ReleaseDC(IntPtr dc);

		[Flags]
		public enum StandardRights : uint {
			Delete = 0x00010000,
			ReadControl = 0x00020000,
			WriteDac = 0x00040000,
			WriteOwner = 0x00080000,
			Synchronize = 0x00100000,
			Required = 0x000f0000,
			Read = ReadControl,
			Write = ReadControl,
			Execute = ReadControl,
			All = 0x001f0000,

			SpecificRightsAll = 0x0000ffff,
			AccessSystemSecurity = 0x01000000,
			MaximumAllowed = 0x02000000,
			GenericRead = 0x80000000,
			GenericWrite = 0x40000000,
			GenericExecute = 0x20000000,
			GenericAll = 0x10000000
		}

		[Flags]
		public enum TokenAccess : uint {
			AssignPrimary = 0x0001,
			Duplicate = 0x0002,
			Impersonate = 0x0004,
			Query = 0x0008,
			QuerySource = 0x0010,
			AdjustPrivileges = 0x0020,
			AdjustGroups = 0x0040,
			AdjustDefault = 0x0080,
			AdjustSessionId = 0x0100,
			All = StandardRights.Required | AssignPrimary | Duplicate | Impersonate |
				Query | QuerySource | AdjustPrivileges | AdjustGroups | AdjustDefault |
				AdjustSessionId,
			GenericRead = StandardRights.Read | Query,
			GenericWrite = StandardRights.Write | AdjustPrivileges | AdjustGroups | AdjustDefault,
			GenericExecute = StandardRights.Execute
		}

		public enum SECURITY_IMPERSONATION_LEVEL {
			SecurityAnonymous,
			SecurityIdentification,
			SecurityImpersonation,
			SecurityDelegation
		}

		public enum TokenType {
			Primary = 1,
			Impersonation
		}

		public enum TOKEN_INFORMATION_CLASS {
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics, // 10
			TokenRestrictedSids,
			TokenSessionId,
			TokenGroupsAndPrivileges,
			TokenSessionReference,
			TokenSandBoxInert,
			TokenAuditPolicy,
			TokenOrigin,
			TokenElevationType,
			TokenLinkedToken,
			TokenElevation, // 20
			TokenHasRestrictions,
			TokenAccessInformation,
			TokenVirtualizationAllowed,
			TokenVirtualizationEnabled,
			TokenIntegrityLevel,
			TokenUIAccess,
			TokenMandatoryPolicy,
			TokenLogonSid,
			MaxTokenInfoClass  // MaxTokenInfoClass should always be the last enum
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ProcessInformation {
			public IntPtr ProcessHandle;
			public IntPtr ThreadHandle;
			public int ProcessId;
			public int ThreadId;
		}

		[Flags]
		public enum ProcessCreationFlags : uint {
			DebugProcess = 0x1,
			DebugOnlyThisProcess = 0x2,
			CreateSuspended = 0x4,
			DetachedProcess = 0x8,
			CreateNewConsole = 0x10,
			NormalPriorityClass = 0x20,
			IdlePriorityClass = 0x40,
			HighPriorityClass = 0x80,
			RealtimePriorityClass = 0x100,
			CreateNewProcessGroup = 0x200,
			CreateUnicodeEnvironment = 0x400,
			CreateSeparateWowVdm = 0x800,
			CreateSharedWowVdm = 0x1000,
			CreateForceDos = 0x2000,
			BelowNormalPriorityClass = 0x4000,
			AboveNormalPriorityClass = 0x8000,
			StackSizeParamIsAReservation = 0x10000,
			InheritCallerPriority = 0x20000,
			CreateProtectedProcess = 0x40000,
			ExtendedStartupInfoPresent = 0x80000,
			ProcessModeBackgroundBegin = 0x100000,
			ProcessModeBackgroundEnd = 0x200000,
			CreateBreakawayFromJob = 0x1000000,
			CreatePreserveCodeAuthzLevel = 0x2000000,
			CreateDefaultErrorMode = 0x4000000,
			CreateNoWindow = 0x8000000,
			ProfileUser = 0x10000000,
			ProfileKernel = 0x20000000,
			ProfileServer = 0x40000000,
			CreateIgnoreSystemDefault = 0x80000000
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct StartupInfo {
			public int Size;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string Reserved;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string Desktop;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string Title;
			public int X;
			public int Y;
			public int XSize;
			public int YSize;
			public int XCountChars;
			public int YCountChars;
			public int FillAttribute;
			public StartupFlags Flags;
			public short ShowWindow;
			public short Reserved2;
			public IntPtr Reserved3;
			public IntPtr StdInputHandle;
			public IntPtr StdOutputHandle;
			public IntPtr StdErrorHandle;
		}

		[Flags]
		public enum StartupFlags : uint {
			UseShowWindow = 0x1,
			UseSize = 0x2,
			UsePosition = 0x4,
			UseCountChars = 0x8,
			UseFillAttribute = 0x10,
			RunFullScreen = 0x20,
			ForceOnFeedback = 0x40,
			ForceOffFeedback = 0x80,
			UseStdHandles = 0x100,
			UseHotkey = 0x200
		}


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern UInt32 WTSGetActiveConsoleSessionId();

		[DllImport("wtsapi32.dll", SetLastError=true)]
		public static extern int WTSQueryUserToken(UInt32 SessionId, out IntPtr Token);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DuplicateTokenEx([In] IntPtr ExistingToken, [In] TokenAccess DesiredAccess, [In] [Optional] IntPtr TokenAttributes, [In] SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, [In] TokenType TokenType, [Out] out IntPtr NewToken);

		[DllImport("ntdll.dll")]
		public static extern int NtSetInformationToken([In] IntPtr TokenHandle, [In] TOKEN_INFORMATION_CLASS TokenInformationClass, [In] ref IntPtr TokenInformation, [In] int TokenInformationLength);

		[DllImport("userenv.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateEnvironmentBlock([Out] out IntPtr Environment, [In] IntPtr TokenHandle, [In] bool Inherit);

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateProcessAsUser(
			[In] [Optional] IntPtr TokenHandle,
			[In] [Optional] string ApplicationName,
			[Optional] string CommandLine,
			[In] [Optional] IntPtr ProcessAttributes,
			[In] [Optional] IntPtr ThreadAttributes,
			[In] bool InheritHandles,
			[In] ProcessCreationFlags CreationFlags,
			[In] [Optional] IntPtr Environment,
			[In] [Optional] string CurrentDirectory,
			[In] ref StartupInfo StartupInfo,
			[Out] out ProcessInformation ProcessInformation);

		[DllImport("kernel32.dll")]
		public static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

		public enum PROCESS_PRIORITY {
			IDLE = 0x00000040,
			NORMAL = 0x00000020,
			HIGH = 0x00000080,
			REAL_TIME = 0x00000100
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

		[StructLayout(LayoutKind.Sequential)]
		public struct ClientId {
			public ClientId(int ProcessId, int ThreadId) {
				UniqueProcess = new IntPtr(ProcessId);
				UniqueThread = new IntPtr(ThreadId);
			}

			public IntPtr UniqueProcess;
			public IntPtr UniqueThread;

			public int ProcessId {
				get {
					return UniqueProcess.ToInt32();
				}
			}
			public int ThreadId {
				get {
					return UniqueThread.ToInt32();
				}
			}
		}

		// thanks PH
		public static ProcessInformation CreateWin32(IntPtr TokenHandle,
										string ApplicationName,
										string CommandLine,
										bool InheritHandles,
										ProcessCreationFlags CreationFlags,
										IntPtr Environment,
										string CurrentDirectory,
										StartupInfo StartupInfo,
										out WinAPI.ClientId ClientId) {
			ProcessInformation processInformation;
			StartupInfo.Size = Marshal.SizeOf(typeof(StartupInfo));
			if (!CreateProcessAsUser(
				TokenHandle,
				ApplicationName,
				CommandLine,
				IntPtr.Zero,
				IntPtr.Zero,
				InheritHandles,
				CreationFlags,
				Environment,
				CurrentDirectory,
				ref StartupInfo,
				out processInformation
				)) {
				ClientId = new ClientId();
				return processInformation;
			}
			ClientId = new ClientId(processInformation.ProcessId, processInformation.ThreadId);
			return processInformation;
		}

		[DllImport("ntdll.dll")]
		public static extern int RtlDestroyEnvironment([In] IntPtr Environment);

		[DllImport("user32.dll")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[Flags]
		public enum RedrawWindowFlags : uint
		{
			Invalidate = 0x1,
			InternalPaint = 0x2,
			Erase = 0x4,
			Validate = 0x8,
			NoInternalPaint = 0x10,
			NoErase = 0x20,
			NoChildren = 0x40,
			AllChildren = 0x80,
			UpdateNow = 0x100,
			EraseNow = 0x200,
			Frame = 0x400,
			NoFrame = 0x800
		}

		[DllImport("user32.dll")]
		public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lpRect, IntPtr hrgnUpdate, RedrawWindowFlags flags);

		public struct RECT {
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		[DllImport("user32.dll")]
		public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName,int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, DESKTOP_ACCESS dwDesiredAccess);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, [Out] byte[] pvInfo, uint nLength, out uint lpnLengthNeeded);

		public const int UOI_FLAGS = 1;
		public const int UOI_NAME = 2;
		public const int UOI_TYPE = 3;
		public const int UOI_USER_SID = 4;
		public const int UOI_HEAPSIZE = 5;
		public const int UOI_IO = 6;

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SwitchDesktop(IntPtr hDesktop);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SetThreadDesktop(IntPtr hDesktop);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetThreadDesktop(uint dwThreadId);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern uint GetCurrentThreadId();

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetCurrentThread();

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool CloseDesktop(IntPtr hDesktop);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr OpenEvent(EVENT_ACCESS dwDesiredAccess, bool bInheritHandle, string lpName);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SetPriorityClass(IntPtr handle, PriorityClass priorityClass);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetCurrentProcess();

		public enum PriorityClass : uint
		{
			ABOVE_NORMAL_PRIORITY_CLASS = 0x8000,
			BELOW_NORMAL_PRIORITY_CLASS = 0x4000,
			HIGH_PRIORITY_CLASS = 0x80,
			IDLE_PRIORITY_CLASS = 0x40,
			NORMAL_PRIORITY_CLASS = 0x20,
			PROCESS_MODE_BACKGROUND_BEGIN = 0x100000, // Windows Vista/2008 and higher
			PROCESS_MODE_BACKGROUND_END = 0x200000, // Windows Vista/2008 and higher
			REALTIME_PRIORITY_CLASS = 0x100
		}

		public enum ThreadPriority : int
		{
			/*
			 * Begin background processing mode. The system lowers the resource scheduling priorities of the thread so that it can perform background work without significantly affecting activity in the foreground.
			 * This value can be specified only if hThread is a handle to the current thread. The function fails if the thread is already in background processing mode.
			 */
			THREAD_MODE_BACKGROUND_BEGIN = 0x00010000,
			
			/*
			 * End background processing mode. The system restores the resource scheduling priorities of the thread as they were before the thread entered background processing mode.
			 * This value can be specified only if hThread is a handle to the current thread. The function fails if the thread is not in background processing mode.
			 */
			THREAD_MODE_BACKGROUND_END = 0x00020000,
			
			THREAD_PRIORITY_ABOVE_NORMAL = 1, // Priority 1 point above the priority class.

			THREAD_PRIORITY_BELOW_NORMAL = -1, // Priority 1 point below the priority class.

			THREAD_PRIORITY_HIGHEST = 2, // Priority 2 points above the priority class.

			THREAD_PRIORITY_IDLE = -15, // Base priority of 1 for IDLE_PRIORITY_CLASS, BELOW_NORMAL_PRIORITY_CLASS, NORMAL_PRIORITY_CLASS, ABOVE_NORMAL_PRIORITY_CLASS, or HIGH_PRIORITY_CLASS processes, and a base priority of 16 for REALTIME_PRIORITY_CLASS processes.

			THREAD_PRIORITY_LOWEST = -2, // Priority 2 points below the priority class.

			THREAD_PRIORITY_NORMAL = 0, // Normal priority for the priority class.

			THREAD_PRIORITY_TIME_CRITICAL = 15 // Base priority of 15 for IDLE_PRIORITY_CLASS, BELOW_NORMAL_PRIORITY_CLASS, NORMAL_PRIORITY_CLASS, ABOVE_NORMAL_PRIORITY_CLASS, or HIGH_PRIORITY_CLASS processes, and a base priority of 31 for REALTIME_PRIORITY_CLASS processes.
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SetThreadPriority(IntPtr hThread, ThreadPriority nPriority);

		[Flags]
		public enum WindowStylesEx : uint {
			/// <summary>Specifies a window that accepts drag-drop files.</summary>
			WS_EX_ACCEPTFILES = 0x00000010,

			/// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
			WS_EX_APPWINDOW = 0x00040000,

			/// <summary>Specifies a window that has a border with a sunken edge.</summary>
			WS_EX_CLIENTEDGE = 0x00000200,

			/// <summary>
			/// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering.
			/// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
			/// </summary>
			/// <remarks>
			/// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order using double-buffering.
			/// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects,
			/// but only if the descendent window also has the WS_EX_TRANSPARENT bit set.
			/// Double-buffering allows the window and its descendents to be painted without flicker.
			/// </remarks>
			WS_EX_COMPOSITED = 0x02000000,

			/// <summary>
			/// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark,
			/// the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message.
			/// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
			/// The Help application displays a pop-up window that typically contains help for the child window.
			/// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
			/// </summary>
			WS_EX_CONTEXTHELP = 0x00000400,

			/// <summary>
			/// Specifies a window which contains child windows that should take part in dialog box navigation.
			/// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations
			/// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
			/// </summary>
			WS_EX_CONTROLPARENT = 0x00010000,

			/// <summary>Specifies a window that has a double border.</summary>
			WS_EX_DLGMODALFRAME = 0x00000001,

			/// <summary>
			/// Specifies a window that is a layered window.
			/// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
			/// </summary>
			WS_EX_LAYERED = 0x00080000,

			/// <summary>
			/// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left.
			/// The shell language must support reading-order alignment for this to take effect.
			/// </summary>
			WS_EX_LAYOUTRTL = 0x00400000,

			/// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
			WS_EX_LEFT = 0x00000000,

			/// <summary>
			/// Specifies a window with the vertical scroll bar (if present) to the left of the client area.
			/// The shell language must support reading-order alignment for this to take effect.
			/// </summary>
			WS_EX_LEFTSCROLLBAR = 0x00004000,

			/// <summary>
			/// Specifies a window that displays text using left-to-right reading-order properties. This is the default.
			/// </summary>
			WS_EX_LTRREADING = 0x00000000,

			/// <summary>
			/// Specifies a multiple-document interface (MDI) child window.
			/// </summary>
			WS_EX_MDICHILD = 0x00000040,

			/// <summary>
			/// Specifies a top-level window created with this style does not become the foreground window when the user clicks it.
			/// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
			/// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
			/// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
			/// </summary>
			WS_EX_NOACTIVATE = 0x08000000,

			/// <summary>
			/// Specifies a window which does not pass its window layout to its child windows.
			/// </summary>
			WS_EX_NOINHERITLAYOUT = 0x00100000,

			/// <summary>
			/// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
			/// </summary>
			WS_EX_NOPARENTNOTIFY = 0x00000004,

			/// <summary>Specifies an overlapped window.</summary>
			WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

			/// <summary>Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
			WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

			/// <summary>
			/// Specifies a window that has generic "right-aligned" properties. This depends on the window class.
			/// The shell language must support reading-order alignment for this to take effect.
			/// Using the WS_EX_RIGHT style has the same effect as using the SS_RIGHT (static), ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
			/// </summary>
			WS_EX_RIGHT = 0x00001000,

			/// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
			WS_EX_RIGHTSCROLLBAR = 0x00000000,

			/// <summary>
			/// Specifies a window that displays text using right-to-left reading-order properties.
			/// The shell language must support reading-order alignment for this to take effect.
			/// </summary>
			WS_EX_RTLREADING = 0x00002000,

			/// <summary>Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.</summary>
			WS_EX_STATICEDGE = 0x00020000,

			/// <summary>
			/// Specifies a window that is intended to be used as a floating toolbar.
			/// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
			/// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
			/// If a tool window has a system menu, its icon is not displayed on the title bar.
			/// However, you can display the system menu by right-clicking or by typing ALT+SPACE. 
			/// </summary>
			WS_EX_TOOLWINDOW = 0x00000080,

			/// <summary>
			/// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
			/// To add or remove this style, use the SetWindowPos function.
			/// </summary>
			WS_EX_TOPMOST = 0x00000008,

			/// <summary>
			/// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
			/// The window appears transparent because the bits of underlying sibling windows have already been painted.
			/// To achieve transparency without these restrictions, use the SetWindowRgn function.
			/// </summary>
			WS_EX_TRANSPARENT = 0x00000020,

			/// <summary>Specifies a window that has a border with a raised edge.</summary>
			WS_EX_WINDOWEDGE = 0x00000100
		}

		[Flags]
		public enum WindowStyles : uint {
			WS_BORDER = 0x00800000, // The window has a thin-line border.
			WS_CAPTION = 0x00C00000, // The window has a title bar (includes the WS_BORDER style).
			WS_CHILD = 0x40000000, // The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
			WS_CHILDWINDOW = 0x40000000, // Same as the WS_CHILD style.
			WS_CLIPCHILDREN = 0x02000000, // Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.
			WS_CLIPSIBLINGS = 0x04000000, // Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated. If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
			WS_DISABLED = 0x08000000, // The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.
			WS_DLGFRAME = 0x00400000, // The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
			WS_GROUP = 0x00020000, // The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style. The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
			WS_HSCROLL = 0x00100000, // The window has a horizontal scroll bar.
			WS_ICONIC = 0x20000000, // The window is initially minimized. Same as the WS_MINIMIZE style.
			WS_MAXIMIZE = 0x01000000, // The window is initially maximized.
			WS_MAXIMIZEBOX = 0x00010000, // The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
			WS_MINIMIZE = 0x20000000, // The window is initially minimized. Same as the WS_ICONIC style.
			WS_MINIMIZEBOX = 0x00020000, // The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
			WS_OVERLAPPED = 0x00000000, // The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_TILED style.
			WS_OVERLAPPEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX), // The window is an overlapped window. Same as the WS_TILEDWINDOW style.
			WS_POPUP = 0x80000000, // The windows is a pop-up window. This style cannot be used with the WS_CHILD style.
			WS_POPUPWINDOW = (WS_POPUP | WS_BORDER | WS_SYSMENU), // The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.
			WS_SIZEBOX = 0x00040000, // The window has a sizing border. Same as the WS_THICKFRAME style.
			WS_SYSMENU = 0x00080000, // The window has a window menu on its title bar. The WS_CAPTION style must also be specified.
			WS_TABSTOP = 0x00010000, // The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function. For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
			WS_THICKFRAME = 0x00040000, // The window has a sizing border. Same as the WS_SIZEBOX style.
			WS_TILED = 0x00000000, // The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.
			WS_TILEDWINDOW = (WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX), // The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.
			WS_VISIBLE = 0x10000000, // The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.
			WS_VSCROLL = 0x00200000 // The window has a vertical scroll bar.
		}

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr CreateWindowExW(
			WindowStylesEx dwExStyle, 
			//[MarshalAs(UnmanagedType.LPStr)] string lpClassName,
			ushort regClass,
			[MarshalAs(UnmanagedType.LPStr)] string lpWindowName, 
			WindowStyles dwStyle, 
			int x, 
			int y, 
			int nWidth, 
			int nHeight,
			IntPtr hWndParent, 
			IntPtr hMenu, 
			IntPtr hInstance, 
			IntPtr lpParam);

		[Flags]
		public enum WindowClassStyle : uint {
			CS_BYTEALIGNCLIENT = 0x1000, // Aligns the window's client area on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.
			CS_BYTEALIGNWINDOW = 0x2000, // Aligns the window on a byte boundary (in the x direction). This style affects the width of the window and its horizontal placement on the display.
			CS_CLASSDC = 0x0040, // Allocates one device context to be shared by all windows in the class. Because window classes are process specific, it is possible for multiple threads of an application to create a window of the same class. It is also possible for the threads to attempt to use the device context simultaneously. When this happens, the system allows only one thread to successfully finish its drawing operation.
			CS_DBLCLKS = 0x0008, // Sends a double-click message to the window procedure when the user double-clicks the mouse while the cursor is within a window belonging to the class.
			CS_DROPSHADOW = 0x00020000, // Enables the drop shadow effect on a window. The effect is turned on and off through SPI_SETDROPSHADOW. Typically, this is enabled for small, short-lived windows such as menus to emphasize their Z-order relationship to other windows. Windows created from a class with this style must be top-level windows; they may not be child windows.
			CS_GLOBALCLASS = 0x4000, // Indicates that the window class is an application global class. For more information, see the "Application Global Classes" section of About Window Classes.
			CS_HREDRAW = 0x0002, // Redraws the entire window if a movement or size adjustment changes the width of the client area.
			CS_NOCLOSE = 0x0200, // Disables Close on the window menu.
			CS_OWNDC = 0x0020, // Allocates a unique device context for each window in the class.
			CS_PARENTDC = 0x0080, // Sets the clipping rectangle of the child window to that of the parent window so that the child can draw on the parent. A window with the CS_PARENTDC style bit receives a regular device context from the system's cache of device contexts. It does not give the child the parent's device context or device context settings. Specifying CS_PARENTDC enhances an application's performance.
			CS_SAVEBITS = 0x0800, // Saves, as a bitmap, the portion of the screen image obscured by a window of this class. When the window is removed, the system uses the saved bitmap to restore the screen image, including other windows that were obscured. Therefore, the system does not send WM_PAINT messages to windows that were obscured if the memory used by the bitmap has not been discarded and if other screen actions have not invalidated the stored image. This style is useful for small windows (for example, menus or dialog boxes) that are displayed briefly and then removed before other screen activity takes place. This style increases the time required to display the window, because the system must first allocate memory to store the bitmap.
			CS_VREDRAW = 0x0001 // Redraws the entire window if a movement or size adjustment changes the height of the client area.
		}

		public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WNDCLASSEX {
			public uint cbSize;
			public WindowClassStyle style;
			public IntPtr lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			public string lpszMenuName;
			public string lpszClassName;
			public IntPtr hIconSm;  

			//Use this function to make a new one with cbSize already filled in.
			//For example:
			//var WndClss = WNDCLASSEX.Build()
			public static WNDCLASSEX Build()
			{
				var nw = new WNDCLASSEX();
				nw.cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX));
				return nw;
			}
		}

		[DllImport("user32.dll")]
		public static extern ushort RegisterClassExW([In] ref WNDCLASSEX lpwcx);

		struct POINT {
			int x;
			int y;
		}

		public struct MSG {
			IntPtr hwnd;
			uint message;
			IntPtr wParam;
			IntPtr lParam;
			uint time;
			POINT pt;
		}

		[DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int PeekMessage(out MSG lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);

		[DllImport("user32.dll")]
		public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

		[DllImport("user32.dll")]
		public static extern int TranslateMessage(ref MSG lpMsg);

		[DllImport("user32.dll")]
		public static extern int DispatchMessage(ref MSG lpMsg);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool DestroyWindow(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);
	}
}
