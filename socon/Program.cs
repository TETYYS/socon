#define SWITCHDESKTOP
//#define DEBUGGER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Diagnostics;
using SharpDX.Windows;
using SharpDX.DirectWrite;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using System.ServiceProcess;
using socon.Native;
using System.Text;

namespace socon
{
	static class Program
	{
		public static bool CreateProcessAsSystem(string ApplicationName, int Session, string Desktop, bool Hidden, WinAPI.PROCESS_PRIORITY Priority, bool Suspended, out WinAPI.ProcessInformation ProcInfo) {
			ProcInfo = new WinAPI.ProcessInformation();
			IntPtr hToken = IntPtr.Zero;
			var phandle = WinAPI.OpenProcess(WinAPI.ProcessAccess.All, false, (uint)Process.GetCurrentProcess().Id);
			try {
				WinAPI.OpenProcessToken(phandle, (int)WinAPI.TokenAccess.All, ref hToken);
			} catch {
				return false;
			}
			try {
				IntPtr hDupToken;
				WinAPI.DuplicateTokenEx(hToken,
				WinAPI.TokenAccess.All,
				IntPtr.Zero,
				WinAPI.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
				WinAPI.TokenType.Primary, out hDupToken);
				Marshal.Release(hToken);
				hToken = hDupToken;
			} catch {
				return false;
			}
			var sessPtr = new IntPtr(Session);
			WinAPI.NtSetInformationToken(hToken, WinAPI.TOKEN_INFORMATION_CLASS.TokenSessionId, ref sessPtr, sizeof(int));
			var startupInfo = new WinAPI.StartupInfo();
			IntPtr environment;
			WinAPI.CreateEnvironmentBlock(out environment, hToken, false);
			startupInfo.Desktop = Desktop;//"WinSta0\\Default";
			WinAPI.ProcessCreationFlags priorityFlag;
			if (Priority == WinAPI.PROCESS_PRIORITY.REAL_TIME) {
				priorityFlag = WinAPI.ProcessCreationFlags.RealtimePriorityClass;
			} else if (Priority == WinAPI.PROCESS_PRIORITY.HIGH) {
				priorityFlag = WinAPI.ProcessCreationFlags.HighPriorityClass;
			} else if (Priority == WinAPI.PROCESS_PRIORITY.IDLE) {
				priorityFlag = WinAPI.ProcessCreationFlags.IdlePriorityClass;
			} else {
				priorityFlag = WinAPI.ProcessCreationFlags.NormalPriorityClass;
			}
			try {
				WinAPI.ClientId clientId;
				ProcInfo = WinAPI.CreateWin32(hToken,
										null,
										ApplicationName,
										false,
										WinAPI.ProcessCreationFlags.CreateUnicodeEnvironment
										| (Hidden ? WinAPI.ProcessCreationFlags.CreateNoWindow : 0x0)
										| (Suspended ? WinAPI.ProcessCreationFlags.CreateSuspended : 0x0)
										| priorityFlag,
										environment,
										null,
										startupInfo,
										out clientId);
				Marshal.Release(ProcInfo.ProcessHandle);
				Marshal.Release(ProcInfo.ThreadHandle);
			} catch {
				return false;
			} finally {
				WinAPI.RtlDestroyEnvironment(environment);
			}
			return true;
		}

		static void Main()
		{
			Environment.CurrentDirectory = @"C:\Users\User\Documents\Visual Studio 2017\Projects\socon\socon\bin\Debug";
			uint dwSessionId;
			WinAPI.ProcessIdToSessionId((uint)Process.GetCurrentProcess().Id, out dwSessionId);
			if (dwSessionId == 0)
				ServiceBase.Run(new Svc());
			
#if DEBUGGER
			while (!Debugger.IsAttached)
				Thread.Sleep(100);
#endif

			//WinAPI.SetPriorityClass(WinAPI.GetCurrentProcess(), WinAPI.PriorityClass.REALTIME_PRIORITY_CLASS);

			AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
			//Application.ThreadException += NBug.Handler.ThreadException;
			Settings.LoadSettings();
			soconwnt.EnablePrivileges();
			DesktopSwitch.Init();
			ETW.Listener.Listen();
			Commands.Commands.PopulateCommands();
			Hooks.InstallHooks();
			Base.CurrentKeyboardInput = new Keyboard.KeyboardFilter();
			Base.CurrentKeyboardInput.PressedInHoldTime = TimeSpan.FromMilliseconds(Settings.Keyboard.PressedInHoldTime);
			Base.CurrentKeyboardInput.PressedInInterval = TimeSpan.FromMilliseconds(Settings.Keyboard.PressedInInterval);
			new Thread(Base.CurrentKeyboardInput.ThreadStart).Start();
			Render.ImportantMessage.Instance = new Render.ImportantMessage();
			Render.ImportantMessage.Instance.Start();
			Render.DefaultSource.Instance = new Render.DefaultSource();
			Plugins.Init.Load();

			new Thread(() => {
				for (;;) {
					if (!Base.TheBox) {
						Thread.Sleep(1000);
						continue;
					}

					/*if (WinAPI.GetForegroundWindow() != Base.hWnd) {
						Console.WriteLine("SWITCH!");
						Base.Exit();
						Base.InitShow();
						aa++;
						if (aa > 50)
							Debugger.Break();
						return;
					}*/
					
					List<IntPtr> windows = new List<IntPtr>();

					/*WinAPI.EnumWindows((IntPtr wnd, IntPtr param) => {
						int dwProcessId;
						//WinAPI.GetWindowThreadProcessId(wnd, out dwProcessId);
						//if (Process.GetProcessById(dwProcessId).ProcessName == "csrss")
						windows.Add(wnd);
						if (wnd == new IntPtr(0x10010)) {
							Debugger.Break();
						}
						return true;
					}, IntPtr.Zero);*/



					/*var str = new StringBuilder(256);
					int status;
					int dwProcessId;
					var name = new byte[256];
					uint lengthNeeded;

					IntPtr hDesktop = WinAPI.OpenInputDesktop(0, false, WinAPI.DESKTOP_ACCESS.GENERIC_ALL);
					WinAPI.GetUserObjectInformation(hDesktop, WinAPI.UOI_NAME, name, (uint)name.Length, out lengthNeeded);

					IntPtr hOldDesktop = WinAPI.GetThreadDesktop(WinAPI.GetCurrentThreadId());
					WinAPI.SetThreadDesktop(hDesktop);
					var wnd = WinAPI.GetForegroundWindow();
					var desk = WinAPI.GetDesktopWindow();
					
					for (int x = 0;x < 2;x++) {
						WinAPI.GetWindowThreadProcessId(wnd, out dwProcessId);
						if ((status = WinAPI.GetClassName(wnd, str, str.Capacity)) == 0)
							Render.DefaultSource.Instance.PushTextError(String.Format("Failed to get class name of {0:8:X} ({1})", wnd, status));
						else
							Render.DefaultSource.Instance.PushTextNormal(
								String.Format("Desktop {0,8:X} {1} -> {2} window {3,8:X}: {4} ({5})",
									hDesktop.ToInt64(),
									Encoding.UTF8.GetString(name),
									x == 0 ? "Foreground" : "Desktop",
									wnd,
									str.ToString(),
									Process.GetProcessById(dwProcessId).ProcessName));
						wnd = desk;
					}

					WinAPI.SetThreadDesktop(hOldDesktop);*/

					/*foreach (var window in windows) {
						var str = new StringBuilder(256);
						int status;
						int dwProcessId;
						/*WinAPI.GetWindowThreadProcessId(window, out dwProcessId);

						if ((status = WinAPI.GetClassName(window, str, str.Capacity)) == 0)
							Render.DefaultSource.Instance.PushTextError(String.Format("Failed to get class name of {0:8:X} ({1})", window, status));
						else {
							if (str.ToString().StartsWith("#327"))
								Render.DefaultSource.Instance.PushTextCenter(String.Format("Window {0,8:X}: {1} ({2})", window, str.ToString(), Process.GetProcessById(dwProcessId).ProcessName));
							else
								Render.DefaultSource.Instance.PushTextNormal(String.Format("Window {0,8:X}: {1} ({2})", window, str.ToString(), Process.GetProcessById(dwProcessId).ProcessName));
						}*
					}*/

					Thread.Sleep(1000);
				}
			}).Start();
		}

		/*public class MainFormWndHook : Form
		{
			protected override CreateParams CreateParams
			{
				get
				{
					const int WS_EX_TOPMOST = 0x00000008;
					CreateParams param = base.CreateParams;
					//param.ExStyle |= WS_EX_TOPMOST;
					return param;
				}
			}

			protected override void WndProc(ref System.Windows.Forms.Message m)
			{
				base.WndProc(ref m);
				//this.Activate();
			}
		}*/

		private static IntPtr myWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                // All GUI painting must be done here
                /*case WM_PAINT:
                    break;
                case WM_DESTROY:
                    //If you want to shutdown the application, call the next function instead of DestroyWindow
                    //PostQuitMessage(0);
                    break;*/
                default:
                    break;
            }
            return WinAPI.DefWindowProc(hWnd, msg, wParam, lParam);
        }

		public static void RenderInit()
		{
#if SWITCHDESKTOP
			IntPtr hDesktop;
			
			if (Base.hOldDesktop == IntPtr.Zero) {
				Base.hOldDesktop = WinAPI.OpenInputDesktop(0, false, WinAPI.DESKTOP_ACCESS.GENERIC_ALL);
				if (Base.hOldDesktop == IntPtr.Zero)
					Debug.Assert(false, "SOCON##");
				//MessageBox.Show(Marshal.GetLastWin32Error().ToString(), "SOCON##");
			} else
				Debug.WriteLine("?!!!!!!!!!!!!!!!!!!!!!!!! OpenInputDesktop -> " + Base.hOldDesktop);
			if ((hDesktop = soconwnt.ScwntCreateDesktop()) == IntPtr.Zero) {
				Debug.Assert(false, "SOCON# " + Marshal.GetLastWin32Error());
				//MessageBox.Show(Marshal.GetLastWin32Error().ToString(), "SOCON#");
			}
			
			//CreateProcessAsSystem("cmd", 1, "WinSta0\\socon_", false, WinAPI.PROCESS_PRIORITY.NORMAL, false, out WinAPI.ProcessInformation info);
			

			WinAPI.SwitchDesktop(hDesktop);
			if (!WinAPI.SetThreadDesktop(hDesktop)) {
				Render.DefaultSource.Instance.PushTextNormal("SetThreadDesktop failed! " + Marshal.GetLastWin32Error());
				Console.Beep();
			}
#endif

			var wcx = WinAPI.WNDCLASSEX.Build();
			wcx.style = WinAPI.WindowClassStyle.CS_NOCLOSE/* | WinAPI.WindowClassStyle.CS_HREDRAW | WinAPI.WindowClassStyle.CS_VREDRAW*/;
			wcx.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(new WinAPI.WndProc(myWndProc));
			wcx.cbClsExtra = 0;
			wcx.cbWndExtra = 0;
			wcx.hInstance = Process.GetCurrentProcess().Handle;
			wcx.hIcon = IntPtr.Zero;
			wcx.hCursor = IntPtr.Zero;
			wcx.hbrBackground = (IntPtr)2; // Black
			wcx.lpszMenuName = null;
			wcx.lpszClassName = "socon";
			wcx.hIconSm = IntPtr.Zero;

			ushort regClass;
			regClass = WinAPI.RegisterClassExW(ref wcx);
			var err = Marshal.GetLastWin32Error();
			Debug.Assert(regClass != 0, "RegisterClassEx, Last err: " + err);

			Base.hWnd = WinAPI.CreateWindowExW(
				WinAPI.WindowStylesEx.WS_EX_CONTEXTHELP/* |
				WinAPI.WindowStylesEx.WS_EX_TOPMOST*/,
				regClass,
				"socon",
				WinAPI.WindowStyles.WS_POPUP | WinAPI.WindowStyles.WS_MINIMIZE,
				0,
				0,
				Screen.ScreenSize.Width,
				Screen.ScreenSize.Height,
				IntPtr.Zero,
				IntPtr.Zero,
				wcx.hInstance,
				IntPtr.Zero);

			Debug.Assert(Base.hWnd != IntPtr.Zero, "CreateWindowEx, Last err: " + Marshal.GetLastWin32Error());

			/*Base.Wnd = new MainFormWndHook();
			Base.Wnd.FormBorderStyle = FormBorderStyle.None;
			Base.Wnd.Location = new Point(0, 0);
			Base.Wnd.Size = Screen.ScreenSize;
			Base.Wnd.ShowInTaskbar = false;
			Base.hWnd = Base.Wnd.Handle;*/

			Base.D2DFactory = new SharpDX.Direct2D1.Factory();
			Base.DWFactory = new SharpDX.DirectWrite.Factory();

			Base.DefaultTextFormat = new TextFormat(
				Base.DWFactory,
				Settings.Text.FontFamily,
				FontWeight.Normal,
				SharpDX.DirectWrite.FontStyle.Normal,
				FontStretch.Normal,
				Settings.Text.FontSize) {
				TextAlignment = TextAlignment.Leading,
				ParagraphAlignment = ParagraphAlignment.Near
			};

			var desc = new SwapChainDescription() {
				BufferCount = 1,
				ModeDescription = new ModeDescription(Screen.ScreenSize.Width, Screen.ScreenSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = new RawBool(true),
				OutputHandle = Base.hWnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput,
				Flags = SwapChainFlags.AllowModeSwitch
			};

			SharpDX.Direct3D11.Device.CreateWithSwapChain(
				DriverType.Hardware,
				DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
				new SharpDX.Direct3D.FeatureLevel[] {
					SharpDX.Direct3D.FeatureLevel.Level_10_0
				},
				desc,
				out Base.DXDevice,
				out Base.DXSwapChain
			);

			Base.DXBackBuffer = Texture2D.FromSwapChain<Texture2D>(Base.DXSwapChain, 0);

			Surface surface = Base.DXBackBuffer.QueryInterface<Surface>();

			Base.D2DRenderTarget = new RenderTarget(
				Base.D2DFactory,
				surface,
				new RenderTargetProperties(
					new PixelFormat(
						Format.Unknown,
						SharpDX.Direct2D1.AlphaMode.Premultiplied
					)
				)
			);

			surface.Dispose();

			WinAPI.ShowWindow(Base.hWnd, 1);
			while (true) {
				try {
					Base.DXSwapChain.SetFullscreenState(new RawBool(true), null);
					break;
				} catch { Thread.Sleep(50); }
			}

			Base.Brushes.InitBrushes();

			if (!Base.FirstInit) {
				Render.Elements.AllElements.ForEach(x => x.RebaseDX());
			} else {
				// First init
				var fpsRenderer = new Render.FPS();
				var topLeft = new Render.TopLeftElements();
				topLeft.AddElement(fpsRenderer);
				Base.CurrentKeyboardInput.SwitchReceiver(Render.DefaultSource.Instance);
				Base.BoxListeners.Add(Render.DefaultSource.Instance);
				Render.Elements.Add(Render.DefaultSource.Instance);
				Render.Elements.Add(topLeft);
			}

			Base.FirstInit = false;
			Base.EvTheBox.Set();
			Base.TheBox = true;

			for (;;) {
				if (!Base.TheBox) {
					// DIE
					Base.DXSwapChain.SetFullscreenState(new RawBool(false), null);
					Debug.Assert(WinAPI.DestroyWindow(Base.hWnd));
					Debug.Assert(WinAPI.UnregisterClass("socon", Process.GetCurrentProcess().Handle));
					Base.hWnd = IntPtr.Zero;
					Base.DefaultTextFormat.Dispose();
					Base.DefaultTextFormat = null;
					Base.D2DRenderTarget.Dispose();
					Base.D2DRenderTarget = null;
					Base.D2DFactory.Dispose();
					Base.D2DFactory = null;
					Base.DWFactory.Dispose();
					Base.DWFactory = null;
					Base.DXBackBuffer.Dispose();
					Base.DXBackBuffer = null;
					Base.DXDevice.ImmediateContext.ClearState();
					Base.DXDevice.ImmediateContext.Flush();
					Base.DXDevice.Dispose();
					Base.DXDevice = null;
					Base.DXSwapChain.Dispose();
					Base.DXSwapChain = null;
					GC.Collect();
					foreach (var list in Base.BoxListeners)
						list.BoxSwitched(false);

#if SWITCHDESKTOP
					List<IntPtr> hwnds = new List<IntPtr>();
					int self = Process.GetCurrentProcess().Id;
					WinAPI.EnumWindows((IntPtr hWnd, IntPtr lParam) => {
						WinAPI.GetWindowThreadProcessId(hWnd, out int procId);
						if (procId == self)
							hwnds.Add(hWnd);
						return true;
					}, IntPtr.Zero);
					foreach (var hWnd in hwnds) {
						WinAPI.DestroyWindow(hWnd);
						var className = new StringBuilder(255);
						WinAPI.GetClassName(hWnd, className, className.Capacity);
						WinAPI.UnregisterClass(className.ToString(), Process.GetCurrentProcess().Handle);
					}
					
					WinAPI.SwitchDesktop(Base.hOldDesktop);
					new Thread(() => {
						Base.RenderThread.Join();
						Debug.WriteLine("Main CloseDesktop");
						if (!WinAPI.CloseDesktop(hDesktop)) {
							var a = Marshal.GetLastWin32Error();
							var b = 2;
						} else {
							Debug.WriteLine("!!!!!@@!!!!!!!!!!!!! CloseDesktop " + hDesktop);
						}
					}).Start();
#endif
					Base.EvTheBox.Set();
					return;
				}
				
				const int WM_DISPLAYCHANGE = 0x007E;
				const int OCM_BASE = 0x2000;

				/*while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 0) != 0) {
					if (GetMessage(out msg, IntPtr.Zero, 0, 0) == -1)
						continue;

					if (msg.msg == OCM_BASE + WM_DISPLAYCHANGE)
						Debug.WriteLine("DISPLAY CHANGE: " + msg.lParam + " " + msg.wParam);

					//var message = new System.Windows.Forms.Message() { HWnd = msg.handle, LParam = msg.lParam, Msg = (int)msg.msg, WParam = msg.wParam };
					TranslateMessage(ref msg);
					DispatchMessage(ref msg);
				}*/
				WinAPI.MSG msg;
				if (WinAPI.PeekMessage(out msg, IntPtr.Zero, 0, 0, 1) != 0) {
					WinAPI.TranslateMessage(ref msg);
					WinAPI.DispatchMessage(ref msg);
				}
				Draw();

				/*WinAPI.MSG msg;
				while (WinAPI.GetMessage(out msg, IntPtr.Zero, 0, 0) != 0) {
					WinAPI.TranslateMessage(ref msg);
					WinAPI.DispatchMessage(ref msg);
					Draw();
				}*/
				//Application.DoEvents();
				
			}
			//RenderLoop.Run(Base.Wnd, Draw, false);
		}

		public static void Draw()
		{
			Base.D2DRenderTarget.BeginDraw();
			Base.D2DRenderTarget.Clear(Base.Brushes.GetColor(Settings.Colors.Background).Color);
			Base.FPS = 1000.0 / (Base.FPSSW.ElapsedTicks / (Stopwatch.Frequency / 1000.0));

			Base.FPSSW.Restart();

			lock (Render.Elements.AllElements) {
				foreach (var el in Render.Elements.AllElements)
					el.Render();
			}
			
			/*LinearGradientBrushProperties prop;
			prop.StartPoint = new RawVector2(0, 0);
			prop.EndPoint = new RawVector2(Screen.ScreenSize.Width, Screen.ScreenSize.Height);
			GradientStop gs1;
			gs1.Color = new RawColor4(0.5f, 0, 0, 255);//Base.Brushes.GetColor(ConsoleColor.DarkRed).Color;
			gs1.Position = 0.0f;
			GradientStop gs2;
			gs2.Color = Base.Brushes.GetColor(ConsoleColor.Red).Color;
			gs2.Position = 1.0f;
			GradientStopCollection col = new GradientStopCollection(Base.D2DRenderTarget, new GradientStop[] {
				gs1, gs2
			});
			Base.D2DRenderTarget.FillRectangle(Screen.ScreenRect, new LinearGradientBrush(Base.D2DRenderTarget, prop, col));*/
			/*TextSource.TextSources.Current.RenderElements();
			TextSource.Renderers.TopLeftElements.Render();
			TextSource.Renderers.Overlay.CurrentOverlay.Render();*/

			Base.D2DRenderTarget.EndDraw();

			var test = Base.DXSwapChain.Present(Settings.Visual.VSync ? 1 : 0, PresentFlags.Test);

			const int D3DERR_DEVICELOST = -1073741819; // 0xC0000005
			/*if (test.Code == D3DERR_DEVICELOST) {
				Base.Exit();
				Base.InitShow();
				return;
			}*/

			var res = Base.DXSwapChain.Present(Settings.Visual.VSync ? 1 : 0, PresentFlags.None);
			if (res.Failure) {
				Debug.WriteLine(res.Code);
			}
		}
	}
}
