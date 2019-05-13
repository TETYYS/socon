using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static socon.Native.WinAPI;

namespace socon
{
	static class Hooks
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate bool RunStopHook(bool State, uint TheCode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void TheBox(bool State, uint TheCode);

		private static IntPtr hHookMod = IntPtr.Zero;
		private static RunStopHook FxRunStopHook;
		private static TheBox FxTheBox;
		public static bool HooksInstalled;

		private static Thread PipeThread;
		private static bool PipeRunning;

		private static List<uint> ProccessIds = new List<uint>();

		public static void PipeServer()
		{
			while (PipeRunning) {
				using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("hkpipe")) {
					Debug.WriteLine("[Server] Pipe created {0}", pipeStream.GetHashCode());
					pipeStream.ReadMode = PipeTransmissionMode.Byte;
					// Wait for a connection
					pipeStream.WaitForConnection();

					if (!PipeRunning)
						return;

					Console.WriteLine("[Server] Pipe connection established");

					using (BinaryReader b = new BinaryReader(pipeStream)) {
						var id = b.ReadUInt32();
						ProccessIds.Add(id);

						Render.DefaultSource.Instance.PushTextNormal("Suspend " + id + " (" + Process.GetProcessById((int)id).ProcessName + ")");

						if (Process.GetProcessById((int)id).ProcessName != "ProcessHacker" && Process.GetProcessById((int)id).ProcessName != "devenv") {
							Render.DefaultSource.Instance.PushTextNormal("Whitelist");
							var hProcess = OpenProcess(ProcessAccess.All, false, id);
							if (hProcess != IntPtr.Zero) {
								NtSuspendProcess(hProcess);
							} else {
								Debug.WriteLine("Failed to open process (ID " + id + ")");
							}
						}
					}
				}
			}
		}

		public static void SwitchTheBox(bool State)
		{
			if (FxTheBox == null) {
				Debug.WriteLine("Switch box true");
				IntPtr hFxTheBox = GetProcAddress(hHookMod, "TheBox");
				FxTheBox = (TheBox)Marshal.GetDelegateForFunctionPointer(hFxTheBox, typeof(TheBox));
			}
			if (!State) {
				Debug.WriteLine("Switch box false");
				FxTheBox(false, 0x9C04180B);

				foreach (var pid in ProccessIds) {
					Debug.WriteLine("PID: " + pid);
					Render.DefaultSource.Instance.PushTextNormal("Resume " + pid);
					var hProcess = OpenProcess(ProcessAccess.All, false, pid);
					if (hProcess != IntPtr.Zero) {
						NtResumeProcess(hProcess);
					} else {
						Debug.WriteLine("Failed to open process (ID " + pid + ")");
					}
				}
				ProccessIds.Clear();

				/*var pids = new uint[arrSz];
				var a = FxTheBox(false, 0x9C04180B, pids);

				if (arrSz == 0) {
					Debug.WriteLine("PIDS empty???");
				}

				foreach (var pid in pids) {
					Debug.WriteLine("PID: " + pid);
					var hProcess = OpenProcess(ProcessAccessFlags.All, false, pid);
					if (hProcess != IntPtr.Zero) {
						NtResumeProcess(hProcess);
					} else {
						Debug.WriteLine("Failed to open process (ID " + pid + ")");
					}
					/*var hThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, pid);
					if (hThread != IntPtr.Zero) {
						uint sCount;
						do {
							sCount = ResumeThread(hThread);
							unchecked {
								if (sCount == ((uint)-1)) {
									Debug.WriteLine("Failed to open thread (ID " + pid + ")");
									continue;
								}
							}
						} while (sCount != 0);
					} else {
						Debug.WriteLine("Failed to open thread (ID " + pid + ")");
					}*
				}*/
			} else {
				FxTheBox(true, 0x9C04180B);
			}
		}

		public static bool InstallHooks()
		{
			if (hHookMod == IntPtr.Zero) {
				hHookMod = LoadLibrary("soconhook.dll");
				if (hHookMod == IntPtr.Zero) {
					int errorCode = Marshal.GetLastWin32Error();
					Debug.WriteLine("Failed to load library (ErrorCode: " + errorCode + ")");
				}
			}
			if (FxRunStopHook == null) {
				IntPtr hFxHook = GetProcAddress(hHookMod, "RunStopHook");
				FxRunStopHook = (RunStopHook)Marshal.GetDelegateForFunctionPointer(hFxHook, typeof(RunStopHook));
			}
			//HooksInstalled = FxRunStopHook(true, 0x133F062A);
			Debug.WriteLine("hooks installed");

			/*PipeThread = new Thread(PipeServer);
			PipeRunning = true;
			PipeThread.Start();*/
			return HooksInstalled;
		}

		public static bool RemoveHooks()
		{
			if (FxRunStopHook == null)
				return false;

			PipeRunning = false;
			//HooksInstalled = FxRunStopHook(false, 0x133F062A);
			return !HooksInstalled;
		}
	}
}
