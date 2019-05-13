using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Native
{
	public static class DesktopSwitch
	{
		private static IntPtr hEvDesktopSwitch;
		private static AutoResetEvent EvDesktopSwitch;
		private static Dictionary<uint, IntPtr> Threads = new Dictionary<uint, IntPtr>();
		public static IntPtr hCurInputDesktop = IntPtr.Zero;
		private static object Lock = new object();
		private static ManualResetEvent EvCurInputFree = new ManualResetEvent(false);

		public static string GetDesktopName(IntPtr hDesktop)
		{
			var str = new byte[255];
			uint len;
			WinAPI.GetUserObjectInformation(hDesktop, WinAPI.UOI_NAME, str, 255, out len);
			if (len > 255) {
				str = new byte[len];
				WinAPI.GetUserObjectInformation(hDesktop, WinAPI.UOI_NAME, str, len, out len);
			}
			return Encoding.Unicode.GetString(str.Take((int)len - 2).ToArray());
		}

		public static bool DesktopEquals(IntPtr a, IntPtr b)
		{
			return GetDesktopName(a) == GetDesktopName(b);
		}

		public static void Init()
		{
			hEvDesktopSwitch = WinAPI.OpenEvent(WinAPI.EVENT_ACCESS.SYNCHRONIZE, true, "WinSta0_DesktopSwitch");
			if (hEvDesktopSwitch == IntPtr.Zero)
				Debug.Assert(false, @"\BaseNamedObjects\WinSta0_DesktopSwitch: " + Marshal.GetLastWin32Error().ToString());

			EvDesktopSwitch = new AutoResetEvent(false);
			EvDesktopSwitch.Close();
			EvDesktopSwitch.SafeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(hEvDesktopSwitch, true);
			new Thread(() => {
				for (;;) {
					IntPtr curDesk;
					IntPtr prevDesk;
					lock (Lock)
						curDesk = hCurInputDesktop;
					prevDesk = curDesk;

					curDesk = WinAPI.OpenInputDesktop(0, false, WinAPI.DESKTOP_ACCESS.DESKTOP_READOBJECTS);

					if (curDesk == IntPtr.Zero) {
						Debug.WriteLine("hCurInputDesktop: " + Marshal.GetLastWin32Error());
						for (int x = 0;x < 60 && curDesk == IntPtr.Zero;x++) {
							curDesk = WinAPI.OpenInputDesktop(0, false, WinAPI.DESKTOP_ACCESS.DESKTOP_READOBJECTS);
							Debug.WriteLine("try hCurInputDesktop -> " + curDesk);
							Thread.Sleep(1000);
						}
						if (curDesk != IntPtr.Zero)
							Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!! OpenInputDesktop -> " + curDesk + " " + GetDesktopName(curDesk));
					} else
						Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!! OpenInputDesktop -> " + curDesk + " " + GetDesktopName(curDesk));

					lock (Lock)
						hCurInputDesktop = curDesk;

					if (prevDesk != IntPtr.Zero) {
						WaitAllForDesktopNot(prevDesk);
						Debug.WriteLine("!!!!!!!!!!!! CloseDesktop " + prevDesk + " " + GetDesktopName(prevDesk));
						if (!WinAPI.CloseDesktop(prevDesk))
							Debug.WriteLine("!!!!!!!!!!!!!!!!!! CloseDesktop FAIL " + prevDesk + " " + Marshal.GetLastWin32Error());
						else {
							EvCurInputFree.Set();
							Debug.WriteLine("InputFree Set");
						}
					}

					EvDesktopSwitch.WaitOne();
					Debug.WriteLine("Desktop SWITCH!!");
				}
			}).Start();
		}

		public static void WaitForInputFree()
		{
			Debug.WriteLine("InputFree Wait");
			EvCurInputFree.WaitOne();
			EvCurInputFree.Reset();
			Debug.WriteLine("InputFree Waited");
		}

		public static void WaitAllForDesktopNot(IntPtr hDesktop)
		{
			for (int x = 0;x < 600;x++) {
				lock (Lock) {
					foreach (var t in Threads) {
						Debug.WriteLine(t.Key + ": " + GetDesktopName(t.Value));
					}
					if (Threads.All(i => !DesktopEquals(i.Value, hDesktop))) {
						Debug.WriteLine("WaitAllForDesktopNot success");
						break;
					}
				}
				Debug.WriteLine("try WaitAllForDesktopNot");
				Thread.Sleep(100);
			}
			/*lock (Lock)
				Threads.Clear();*/
		}

		public static void WaitAllForDesktop(IntPtr hDesktop)
		{
			for (int x = 0;x < 600;x++) {
				lock (Lock) {
					foreach (var t in Threads) {
						Debug.WriteLine(t.Key + ": " + GetDesktopName(t.Value));
					}
					if (Threads.All(i => DesktopEquals(i.Value, hDesktop))) {
						Debug.WriteLine("WaitAllForDesktop success");
						break;
					}
				}
				Debug.WriteLine("try WaitAllForDesktop");
				Thread.Sleep(100);
			}
			/*lock (Lock)
				Threads.Clear();*/
		}

		public static IntPtr PollAutoDesktopThreadSwitch(uint dwThread)
		{
			//var dwThread = WinAPI.GetCurrentThreadId();

			lock (Lock) {
				if (Threads.ContainsKey(dwThread)) {
					if (!DesktopEquals(Threads[dwThread], hCurInputDesktop)) {
						WinAPI.SetThreadDesktop(hCurInputDesktop);
						Threads[dwThread] = hCurInputDesktop;
						Debug.WriteLine(dwThread + " -> " + hCurInputDesktop + " " + GetDesktopName(hCurInputDesktop));
					}
				} else {
					var hThreadCur = WinAPI.GetThreadDesktop(dwThread);
					if (!DesktopEquals(hCurInputDesktop, hThreadCur))
						WinAPI.SetThreadDesktop(hCurInputDesktop);

					Debug.WriteLine(dwThread + " new-> " + hCurInputDesktop + " " + GetDesktopName(hCurInputDesktop));
					Threads.Add(dwThread, hCurInputDesktop);
				}

				return hCurInputDesktop;
			}
		}
	}
}
