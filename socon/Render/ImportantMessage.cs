using socon.Native;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Render
{
	class ImportantMessage : IRenderer
	{
		public static ImportantMessage Instance;

		private object hDesktopLock = new object();

		private Brush White = new SolidBrush(Color.White);
		private OrderedDictionary Messages = new OrderedDictionary();
		private List<Tuple<IntPtr, DateTime>> MessagesTimeout = new List<Tuple<IntPtr, DateTime>>();
		int Handle = 0;

		public ImportantMessage()
		{

		}

		public void RebaseDX()
		{

		}

		public IntPtr AddMessage(string Text)
		{
			var handle = new IntPtr(Handle);
			Messages.Add(handle, Text.ToUpper());
			unchecked { Handle++; }
			return handle;
		}

		public void AddMessageTimeout(string Text, TimeSpan Timeout)
		{
			var hMsg = AddMessage(Text);
			MessagesTimeout.Add(Tuple.Create(hMsg, DateTime.Now + Timeout));
		}

		public void RemoveMessage(IntPtr Handle)
		{
			Messages.Remove(Handle);
			ForceErase = true;
		}

		uint dwRenderThread;
		bool ForceErase = false;

		public void Start()
		{
			new Thread(() => {
				dwRenderThread = WinAPI.GetCurrentThreadId();
				for (;;) {
					DesktopSwitch.PollAutoDesktopThreadSwitch(dwRenderThread);

					if (Base.TheBox) {
						Thread.Sleep(150);
						continue;
					}

					if (ForceErase) {
						var hWnd = WinAPI.GetDesktopWindow();
						ForceErase = false;
						if (!WinAPI.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, WinAPI.RedrawWindowFlags.Erase | WinAPI.RedrawWindowFlags.Invalidate | WinAPI.RedrawWindowFlags.AllChildren)) {
							Console.Beep();
						}
					}

					if (Messages.Count != 0)
						Render();

					foreach (var msg in MessagesTimeout.ToArray()) {
						if (msg.Item2 < DateTime.Now) {
							RemoveMessage(msg.Item1);
							MessagesTimeout.Remove(msg);
						}
					}

					Thread.Sleep(16);
				}
			}).Start();
		}

		SizeF previousSize;

		public void Render()
		{
			try {
				var str = (string)Messages[Messages.Count - 1];
				var font = new Font("VCR OSD Mono", 32);
				
				var hWnd = WinAPI.GetDesktopWindow();
				var gfx = Graphics.FromHwnd(hWnd);

				var textSz = gfx.MeasureString(str, font, Screen.ScreenSize.Width);

				if (previousSize != null && (previousSize.Height > textSz.Height || previousSize.Width > textSz.Width)) {
					var rect = new WinAPI.RECT();

					var pointPrev = new PointF(
						Screen.ScreenSize.Width / 2 - (previousSize.Width / 2),
						(Screen.ScreenSize.Height / 2) + (Screen.ScreenSize.Height / 4) - (previousSize.Height / 2)
					);
					rect.bottom = (int)Math.Ceiling(pointPrev.Y + previousSize.Height);
					rect.left = (int)Math.Ceiling(pointPrev.X);
					rect.right = (int)Math.Ceiling(pointPrev.X + previousSize.Width);
					rect.top = (int)Math.Ceiling(pointPrev.Y);

					var pRect = Marshal.AllocHGlobal(Marshal.SizeOf(rect)); {
						Marshal.StructureToPtr(rect, pRect, true);
						if (!WinAPI.RedrawWindow(hWnd, pRect, IntPtr.Zero, WinAPI.RedrawWindowFlags.Erase | WinAPI.RedrawWindowFlags.Invalidate | WinAPI.RedrawWindowFlags.AllChildren)) {
							Console.Beep(300, 100);
						}
					} Marshal.FreeHGlobal(pRect);
				}

				var point = new PointF(
					Screen.ScreenSize.Width / 2 - (textSz.Width / 2),
					(Screen.ScreenSize.Height / 2) + (Screen.ScreenSize.Height / 4) - (textSz.Height / 2)
				);

				BufferedGraphicsContext context = BufferedGraphicsManager.Current;
				BufferedGraphics buffer = context.Allocate(gfx, new Rectangle((int)point.X, (int)point.Y, (int)textSz.Width, (int)textSz.Height));
				buffer.Graphics.DrawString(str, font, White, point);

				buffer.Render();

				previousSize = textSz;
			} catch (Exception ex) {
				Debugger.Break();
			}
		}
	}
}
