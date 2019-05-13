using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using socon.Keyboard;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using System.Diagnostics;
using System.Threading;

namespace socon.Render
{
	class WaitingBoxOverlay : IKeyboardInputReceiver, IRenderer, IDisposable
	{
		private TextLayout Text;
		private TextFormat BoxCenterFormat;
		private string sText;
		private RawRectangleF BoxRect;
		private RawVector2 TextOrigin;
		private CancellationTokenSource Cancel;

		private void InitText()
		{
			this.BoxCenterFormat = new TextFormat(Base.DWFactory, Base.DefaultTextFormat.FontFamilyName, Base.DefaultTextFormat.FontWeight, Base.DefaultTextFormat.FontStyle, Base.DefaultTextFormat.FontStretch, Base.DefaultTextFormat.FontSize) {
				TextAlignment = TextAlignment.Center,
				ParagraphAlignment = ParagraphAlignment.Center
			};

			this.Text = new TextLayout(
				Base.DWFactory,
				this.sText,
				Base.DefaultTextFormat,
				Screen.ScreenSize.Width,
				Screen.ScreenSize.Height);
		}

		public WaitingBoxOverlay(string Text, CancellationTokenSource Cancel)
		{
			this.sText = Text;
			this.Cancel = Cancel;
		}

		public void PushKey(char Key)
		{
			if (Key == (char)0x03) {
				Cancel?.Cancel();
				Elements.Remove(this);
			}
		}

		public void RebaseDX()
		{
			this.Text.Dispose();
			this.BoxCenterFormat.Dispose();
			InitText();
		}

		public void Display()
		{
			Debug.Assert(Elements.Exists<DefaultSource>());
			Debug.Assert(!Elements.Exists<WaitingBoxOverlay>());
			
			InitText();
			var boxX = this.Text.Metrics.Width + 40.0f;
			var boxY = this.Text.Metrics.Height + 20.0f;
			BoxRect = new RawRectangleF(
				(Screen.ScreenSize.Width / 2) - (boxX / 2),
				(Screen.ScreenSize.Height / 2) - (boxY / 2),
				(Screen.ScreenSize.Width / 2) + (boxX / 2),
				(Screen.ScreenSize.Height / 2) + (boxY / 2)
			);
			TextOrigin = new RawVector2(BoxRect.Left + 20.0f, BoxRect.Top + 10.0f);
			Elements.AddBefore<ConfirmBoxOverlay>(this);
		}

		public void Dispose()
		{
			Elements.Remove(this);
			this.Text.Dispose();
			this.BoxCenterFormat.Dispose();
		}

		~WaitingBoxOverlay()
		{
			Dispose();
		}

		public void Render()
		{
			var backgr = Base.Brushes.GetColor(Settings.Colors.ConfirmBox.Background);
			var border = Base.Brushes.GetColor(Settings.Colors.ConfirmBox.Border);
			var textColor = Base.Brushes.GetColor(Settings.Colors.ConfirmBox.Text);

			Base.D2DRenderTarget.FillRectangle(BoxRect, backgr);
			Base.D2DRenderTarget.DrawRectangle(BoxRect, border, 2.0f);
			Base.D2DRenderTarget.DrawTextLayout(TextOrigin, Text, textColor);
		}

		public void SpecialKey(VirtualKeys.VK Key) { }
	}
}
