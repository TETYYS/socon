#define PRIVATE

using SharpDX.Direct2D1; 
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using socon.Keyboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using static socon.Keyboard.VirtualKeys;

namespace socon.Render
{
	class DefaultSource : IKeyboardInputReceiver, IRenderer, Base.IBoxListener
	{
		public static DefaultSource Instance;

		TextMetrics spaceMetrics;
		const string CommandPre = "> ";
		bool SpecialCommandPending;
		bool ScrollMode;
		private List<Tuple<string, ConsoleColor, bool>> MessageQueue = new List<Tuple<string, ConsoleColor, bool>>();

		private void GetSpaceMetrics()
		{
			using (var spaceSz = new TextLayout(Base.DWFactory, " ", Base.DefaultTextFormat, Screen.ScreenSize.Width, Screen.ScreenSize.Height))
				spaceMetrics = spaceSz.Metrics;
		}

		public void RebaseDX()
		{
			GetSpaceMetrics();

			lock (ScreenElements) {
				foreach (var el in ScreenElements)
					el.RebaseDX();
			}
		}

		public DefaultSource()
		{
			PushTextNormal("socon " + AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToSt‌​ring() + "\n");
			if (Hooks.HooksInstalled) {
				PushTextNormal("Hooks installed");
				PushTextNormal("Stop all world message pumps enabled");
			}
			PushTextNormal("Keyboard input: " + Base.CurrentKeyboardInput.InputName);
			if (Native.soconwnt.PrivilegesEnabled)
				PushTextNormal("Privileges enabled");
		}

		interface IScreenElement : Base.IDXRebasable
		{
			/// <summary>
			/// Renders screen element
			/// </summary>
			/// <param name="Pos">Position to render</param>
			/// <returns>Rendered element size</returns>
			RawVector2 Render(RawVector2 Pos);

			RawVector2 RenderedElementSize { get; }
		}

		class BitmapElement : IScreenElement
		{
			public Bitmap[] Bitmaps = null;

			Func<Bitmap[]> FxLoadBitmaps;

			private void LoadBitmaps()
			{
				this.Bitmaps = FxLoadBitmaps();
				this.RenderedElementSize = new RawVector2(this.Bitmaps[0].Size.Width, this.Bitmaps[0].Size.Height);
			}

			public BitmapElement(Func<Bitmap[]> LoadBitmaps)
			{
				this.FxLoadBitmaps = LoadBitmaps;
			}

			public void RebaseDX()
			{
				foreach (var bmp in Bitmaps)
					bmp.Dispose();
				LoadBitmaps();
			}

			private byte counter = 0;

			public RawVector2 RenderedElementSize { get; private set; }

			public RawVector2 Render(RawVector2 Pos)
			{
				if (Bitmaps == null)
					LoadBitmaps();

				unchecked {
					counter++;
				}
				var bmp = Bitmaps[counter % 2];
				var mX = Pos.X < 0 ? -Pos.X : 0;
				var mY = Pos.Y < 0 ? -Pos.Y : 0;
				Base.D2DRenderTarget.DrawBitmap(bmp, new RawRectangleF(Pos.X, Pos.Y, bmp.Size.Width - mX, bmp.Size.Height - mY), 1.0f, BitmapInterpolationMode.Linear);
				return new RawVector2(bmp.Size.Width, bmp.Size.Height);
			}

			~BitmapElement()
			{
				foreach (var bmp in Bitmaps)
					bmp.Dispose();

				Bitmaps = null;
			}
		}

		class TextElement : IScreenElement
		{
			public TextLayout Text;
			public TextFormat Format;
			public string TextString;
			public ConsoleColor Color;

			public TextElement(TextLayout Text, TextFormat Format, string TextString, ConsoleColor Color)
			{
				this.Text = Text;
				this.Format = Format;
				this.Color = Color;
				this.TextString = TextString;
				this.RenderedElementSize = new RawVector2(Text.Metrics.Width, Text.Metrics.Height);
			}

			public RawVector2 RenderedElementSize { get; private set; }

			~TextElement()
			{
				Format?.Dispose();
				Text.Dispose();
			}

			public void RebaseDX()
			{
				if (Format == null)
					Text = Base.Rebase.RebaseText(Text, TextString);
				else {
					var rebase = Base.Rebase.RebaseText(Text, Format, TextString);
					Text = rebase.Item1;
					Format = rebase.Item2;
				}
			}

			public RawVector2 Render(RawVector2 Pos)
			{
				Base.D2DRenderTarget.DrawTextLayout(Pos, Text, Base.Brushes.GetColor(Color), DrawTextOptions.None);
				return new RawVector2(Text.Metrics.Width, Text.Metrics.Height);
			}
		}

		private List<IScreenElement> ScreenElements = new List<IScreenElement>();
		private object ScreenElementsLock = new object();
		private float TextElementsHeight = 0;
		private float TextElementsScroll = 0;
		private const int ScrollIntensity = 20;


		private List<string> CommandHistory = new List<string>();
		private object CommandHistoryLock = new object();
		private int CommandHistoryIndex = -1;
		private string Command = "";
		private object CommandLock = new object();
		private int Cursor = 0;
		private object CursorLock = new object();

		private void SetCarretEnd()
		{
			lock (CommandLock) {
				lock (CursorLock) {
					Cursor = Command.Length;
				}
			}
		}

		public void SpecialKey(VK Key)
		{
			switch (Key) {
				case VK.VK_BACK:
					lock (CommandLock) {
						if (Command.Length == 0)
							break;

						lock (CursorLock) {
							SetCommand(Command.Remove(Cursor - 1, 1), Cursor - 1);
						}
					}
					break;
				case VK.VK_DELETE:
					lock (CommandLock) {
						if (Command.Length == 0)
							break;

						lock (CursorLock) {
							SetCommand(Command.Remove(Cursor - 1, 1), Cursor > Command.Length - 1 ? Int32.MaxValue : Cursor);
						}
					}
					break;
				case VK.VK_LEFT:
					lock (CursorLock) {
						if (Cursor > 0)
							Cursor--;
					}
					break;
				case VK.VK_RIGHT:
					lock (CommandLock) {
						lock (CursorLock) {
							if (Cursor < Command.Length)
								Cursor++;
						}
					}
					break;
				case VK.VK_UP:
					if (!ScrollMode) {
						lock (CommandLock) {
							lock (CommandHistoryLock) {
								if (CommandHistoryIndex == -1) {
									SetCommand("", 0);
									break;
								}
								if (CommandHistoryIndex > 0) {
									CommandHistoryIndex--;
									SetCommand(CommandHistory[CommandHistoryIndex], Int32.MaxValue);
								}
							}
						}
					} else {
						lock (ScreenElementsLock) {
							if (TextElementsScroll < (TextElementsHeight - Screen.ScreenSize.Height))
								TextElementsScroll += Math.Min(ScrollIntensity, (TextElementsHeight - Screen.ScreenSize.Height) - TextElementsScroll);
						}
					}
					break;
				case VK.VK_DOWN:
					if (!ScrollMode) {
						lock (CommandLock) {
							lock (CommandHistoryLock) {
								if (CommandHistoryIndex == -1) {
									SetCommand("", 0);
									break;
								}
								CommandHistoryIndex++;
								if (CommandHistoryIndex >= CommandHistory.Count) {
									CommandHistoryIndex = CommandHistory.Count;
									SetCommand("", 0);
									break;
								}
							
								SetCommand(CommandHistory[CommandHistoryIndex], Int32.MaxValue);
							}
						}
					} else {
						lock (ScreenElementsLock) {
							if (TextElementsScroll > 0)
								TextElementsScroll -= Math.Min(ScrollIntensity, TextElementsScroll);
						}
					}
					break;
				case VK.VK_ESCAPE:
					SpecialCommandPending = false;
					ScrollMode = false;
					lock (ScreenElementsLock)
						TextElementsScroll = 0;
					break;
			}
		}

		public void PushKey(char Key)
		{
			if (ScrollMode)
				return;

			if (Key == (char)0x02) {
				SpecialCommandPending = true;
				return;
			}

			if (!SpecialCommandPending) {
				string parseCmd = "";
				lock (CommandLock) {
					var cmd = Command;
				
					if (Key == (char)0x03 || Key == '\r') {
						PushCommandHistory(Command, Key == '\r' ? "" : "^C");
						SetCommand("", 0);
						lock (CommandHistoryLock) {
							if (CommandHistory.Count != 0)
								CommandHistoryIndex = CommandHistory.Count;
						}
					}
					if (Key == '\r')
						parseCmd = cmd;
					else {
						lock (CursorLock) {
							SetCommand(Command.Substring(0, Cursor) + Key + Command.Substring(Cursor), Cursor + 1);
						}
					}
				}
				if (parseCmd != "")
					Task.Run(() => Commands.Parser.ParseCommand(parseCmd));
			} else {
				if (Key == '[') {
					ScrollMode = true;
					SpecialCommandPending = false;
				}
			}
		}

		private void SetCommand(string Cmd, int Cursor)
		{
			lock (CommandLock) {
				lock (CursorLock) {
					Command = Cmd;
					if (Cursor == Int32.MaxValue)
						SetCarretEnd();
					else
						this.Cursor = Cursor;
				}
			}
		}

		private void PushCommandHistory(string PushCmd, string Post)
		{
			PushTextNormal(CommandPre + PushCmd + Post);

			lock (CommandHistoryLock) {
				if (PushCmd == "" ||
					(CommandHistoryIndex != -1 &&
					CommandHistoryIndex != CommandHistory.Count &&
					CommandHistory[CommandHistoryIndex] == PushCmd))
					return;

				CommandHistory.Add(PushCmd);
				CommandHistoryIndex++;
			}
		}

		public void PushTextCenter(string Text)
		{
			if (!Base.TheBox) {
				MessageQueue.Add(Tuple.Create(Text, Settings.Colors.CenterText, true));
				return;
			}

			var thisTf = Base.DefaultTextFormat;
			var tf = new TextFormat(Base.DWFactory, thisTf.FontFamilyName, thisTf.FontWeight, thisTf.FontStyle, thisTf.FontStretch, thisTf.FontSize)
			{
				TextAlignment = TextAlignment.Center,
				ParagraphAlignment = thisTf.ParagraphAlignment
			};
			PushText(Text, Settings.Colors.CenterText, tf);
		}

		public void PushTextError(string Text)
		{
			PushText(Text, Settings.Colors.ErrorText);
		}

		public void PushTextNormal(string Text)
		{
			PushText(Text, Settings.Colors.NormalText);
		}

		public void PushText(string Text, ConsoleColor Color)
		{
			if (!Base.TheBox) {
				MessageQueue.Add(Tuple.Create(Text, Color, false));
				return;
			}
			TextLayout tl = null;
			try {
			tl = new TextLayout(
				Base.DWFactory,
				Text,
				Base.DefaultTextFormat,
				Screen.ScreenSize.Width,
				Screen.ScreenSize.Height);
			} catch (Exception ex) {
				Debugger.Break();
			}
			lock (ScreenElementsLock) {
				ScreenElements.Add(new TextElement(tl, null, Text, Color));
				TextElementsHeight += tl.Metrics.Height;
			}
		}

		private void PushText(string Text, ConsoleColor Color, TextFormat TextFormat)
		{
			TextLayout tl = new TextLayout(
				Base.DWFactory,
				Text,
				TextFormat,
				Screen.ScreenSize.Width,
				Screen.ScreenSize.Height);
			lock (ScreenElementsLock) {
				ScreenElements.Add(new TextElement(tl, TextFormat, Text, Color));
				TextElementsHeight += tl.Metrics.Height;
			}
		}

		public void Render()
		{
			RawVector2 nextPos;
			float cursorY;
			lock (ScreenElementsLock) {
				cursorY = Math.Min(TextElementsHeight, Screen.ScreenSize.Height - (ScrollMode ? 0 : Base.DefaultTextFormat.FontSize));
				nextPos = new RawVector2(0, cursorY);
				float skipSz = 0;
				int x;
				for (x = ScreenElements.Count - 1; skipSz < TextElementsScroll; x--)
					skipSz += ScreenElements[x].RenderedElementSize.Y;

				var diff = TextElementsScroll - skipSz;
				nextPos.Y += diff;

				while (nextPos.Y > 0 && x >= 0) {
					nextPos.Y -= ScreenElements[x].RenderedElementSize.Y;
					ScreenElements[x].Render(nextPos);
					x--;
				}
			}

			TextLayout tl;

			lock (CommandLock) {
				tl = new TextLayout(
					Base.DWFactory,
					CommandPre + Command,
					Base.DefaultTextFormat,
					Screen.ScreenSize.Width,
					Screen.ScreenSize.Height
				);

				Base.D2DRenderTarget.FillRectangle(new RawRectangleF(0, cursorY, tl.Metrics.Width, cursorY + tl.Metrics.Height), Base.Brushes.GetColor(Settings.Colors.Background));

				Base.D2DRenderTarget.DrawTextLayout(
					new RawVector2(0, cursorY),
					tl,
					Base.Brushes.GetColor(Settings.Colors.NormalText),
					DrawTextOptions.Clip
				);
			}

			if (DateTime.Now.Millisecond % 200 >= 100 && !Commands.Parser.CommandExecuting) {
				float x, y;
				lock (CursorLock)
					tl.HitTestTextPosition(Cursor + CommandPre.Length, new RawBool(false), out x, out y);

				//x = tl.Metrics.WidthIncludingTrailingWhitespace;

				Base.D2DRenderTarget.DrawLine(
					new RawVector2(
						x,
						cursorY
					),
					new RawVector2(
						x,
						cursorY + Base.DefaultTextFormat.FontSize
					),
					Base.Brushes.GetColor(Settings.Colors.NormalText)
				);
			}
			tl.Dispose();

			if (SpecialCommandPending || ScrollMode) {
				StrokeStyleProperties ssp = new StrokeStyleProperties();
				ssp.DashStyle = DashStyle.Dash;
				ssp.DashOffset = 15.0f;
				using (var ss = new StrokeStyle(Base.D2DFactory, ssp))
					Base.D2DRenderTarget.DrawRectangle(Screen.ScreenRect, Base.Brushes.GetColor(ScrollMode ? ConsoleColor.DarkBlue : ConsoleColor.DarkRed), 3.0f, ss);
			}
		}

		public void BoxSwitched(bool State)
		{
			if (State) {
				foreach (var msg in MessageQueue) {
					if (msg.Item3)
						PushTextCenter(msg.Item1);
					else
						PushText(msg.Item1, msg.Item2);
				}
				MessageQueue.Clear();
			}
		}

		~DefaultSource()
		{
			lock (ScreenElementsLock) {
				ScreenElements.Clear();
			}
		}
	}
}
