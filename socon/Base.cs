using Microsoft.Diagnostics.Runtime;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace socon
{
	public static class Base
	{
		private static bool _theBox;
		public static bool TheBox {
			get {
				return _theBox;
			}
			set {
				_theBox = value;
				Hooks.SwitchTheBox(value);
				if (value) {
					foreach (var list in BoxListeners)
						list.BoxSwitched(true);
				}
			}
		}
		public static ManualResetEvent EvTheBox = new ManualResetEvent(false);
		public static double FPS;
		public static Stopwatch FPSSW = new Stopwatch();
		public static SharpDX.Direct2D1.Factory D2DFactory;
		public static SharpDX.DirectWrite.Factory DWFactory;
		public static RenderTarget D2DRenderTarget;
		public static SwapChain DXSwapChain;
		public static Texture2D DXBackBuffer;
		public static SharpDX.Direct3D11.Device DXDevice;

		public static IntPtr hWnd;
		public static IntPtr hOldDesktop;

		public static Thread RenderThread;
		public static bool FirstInit = true;

		public static TextFormat DefaultTextFormat;
		public static Keyboard.IKeyboardInput CurrentKeyboardInput;

		public static List<IBoxListener> BoxListeners = new List<IBoxListener>();

		public interface IDXRebasable
		{
			void RebaseDX();
		}

		public interface IBoxListener
		{
			void BoxSwitched(bool State);
		}

		public static void WaitForTheBox()
		{
			if (TheBox)
				return;
			EvTheBox.WaitOne();
		}

		public static void Exit()
		{
			// HACKHACK
			/*ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;

			using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive)) {
				ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
				foreach (ClrThread thread in runtime.Threads) {
					thread.
				}
			}
			Process.Start(Application.ExecutablePath);
			Process.GetCurrentProcess().Kill();*/
			TheBox = false;
			new Thread(() => { Thread.Sleep(40000); EvTheBox.Set(); }).Start();
			EvTheBox.WaitOne();
		}

		public static void InitShow()
		{
			RenderThread = new Thread(Program.RenderInit);
			RenderThread.Start();

			EvTheBox.WaitOne();
			EvTheBox.Reset();
		}

		public static class Rebase
		{
			public static TextLayout RebaseText(TextLayout Tl, string Text)
			{
				Debug.Assert(Base.DWFactory != null);
				Debug.Assert(Base.DefaultTextFormat != null);
				Debug.Assert(Text != null);
				Debug.Assert(Tl != null);

				var tlNew = new TextLayout(
					Base.DWFactory,
					Text,
					Base.DefaultTextFormat,
					Tl.MaxWidth,
					Tl.MaxHeight);
				Tl.Dispose();
				return tlNew;
			}

			public static Tuple<TextLayout, TextFormat> RebaseText(TextLayout Tl, TextFormat Tf, string Text)
			{
				Tf.Dispose();
				var tfNew = new TextFormat(
					Base.DWFactory,
					Tl.FontFamilyName,
					Tl.FontCollection,
					Tl.FontWeight,
					Tl.FontStyle,
					Tl.FontStretch,
					Tl.FontSize,
					Tl.LocaleName
				) {
					TextAlignment = Tl.TextAlignment,
					ParagraphAlignment = Tl.ParagraphAlignment,
					FlowDirection = Tl.FlowDirection,
					IncrementalTabStop = Tl.IncrementalTabStop,
					ReadingDirection = Tl.ReadingDirection,
					WordWrapping = Tl.WordWrapping
				};

				var tlNew = new TextLayout(
					Base.DWFactory,
					Text,
					tfNew,
					Tl.MaxWidth,
					Tl.MaxHeight);
				Tl.Dispose();
				return Tuple.Create(tlNew, tfNew);
			}
		}

		public static class Brushes
		{
			private static List<SolidColorBrush> brushes = new List<SolidColorBrush>();
			private static List<SolidColorBrush> negativeBrushes = new List<SolidColorBrush>();

			private static int[][] colorMap = new int[][] {
				new int[3] { 0, 0, 0 },
				new int[3] { 0, 0, 0x8B },
				new int[3] { 0, 0x64, 0 },
				new int[3] { 0, 0x8B, 0x8B },
				new int[3] { 0x8B, 0, 0 },
				new int[3] { 0x8B, 0, 0x8B },
				new int[3] { 0xD7, 0xC3, 0x2A },
				new int[3] { 0x80, 0x80, 0x80 },
				new int[3] { 0xA9, 0xA9, 0xA9 },
				new int[3] { 0, 0, 0xFF },
				new int[3] { 0, 0x80, 0 },
				new int[3] { 0, 0xFF, 0xFF },
				new int[3] { 0xFF, 0, 0 },
				new int[3] { 0xFF, 0, 0xFF },
				new int[3] { 0xFF, 0xFF, 0 },
				new int[3] { 0xFF, 0xFF, 0xFF }
			};

			public static int[] ConsoleColorToRGB(ConsoleColor Color)
			{
				var colors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
				for (int x = 0; x < colors.Count; x++) {
					if (colors[x] == Color)
						return colorMap[x];
				}
				throw new KeyNotFoundException();
			}

			public static void InitBrushes()
			{
				if (brushes.Count != 0) {
					foreach (var brush in brushes)
						brush.Dispose();
					foreach (var brush in negativeBrushes)
						brush.Dispose();
					brushes.Clear();
					negativeBrushes.Clear();
				}

				var colors = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToList();
				for (int x = 0; x < colors.Count; x++) {
					brushes.Add(new SolidColorBrush(D2DRenderTarget, new SharpDX.Mathematics.Interop.RawColor4(colorMap[x][0] / 255.0f, colorMap[x][1] / 255.0f, colorMap[x][2] / 255.0f, 255)));
					negativeBrushes.Add(new SolidColorBrush(D2DRenderTarget, new SharpDX.Mathematics.Interop.RawColor4((0xFF - colorMap[x][0]) / 255.0f, (0xFF - colorMap[x][1]) / 255.0f, (0xFF - colorMap[x][2]) / 255.0f, 255)));
				}
			}

			public static ConsoleColor GetColorByString(string Color)
			{
				return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), Color);
			}

			public static SolidColorBrush GetNegativeColor(ConsoleColor Color)
			{
				return negativeBrushes[(int)Color];
			}

			public static SolidColorBrush GetColor(ConsoleColor Color)
			{
				return brushes[(int)Color];
			}
		}
	}
}
