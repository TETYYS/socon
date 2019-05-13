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
	class ConfirmBoxOverlay : IKeyboardInputReceiver, IRenderer
	{
		private TextLayout Text;
		private TextFormat BoxCenterFormat;
		private string sText;
		private bool OnYes = false;
		private RawRectangleF BoxRect;
		private RawRectangleF YesRect;
		private RawRectangleF NoRect;
		private RawVector2 TextOrigin;
		SemaphoreSlim answerWait = new SemaphoreSlim(0, 1);
		private bool BoxNegative;

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

		public ConfirmBoxOverlay(string Text)
		{
			Debug.Assert(Elements.Exists<DefaultSource>());

			this.sText = Text;
			InitText();
			var boxX = this.Text.Metrics.Width + 40.0f;
			var boxY = this.Text.Metrics.Height + 20.0f;
			BoxRect = new RawRectangleF(
				(Screen.ScreenSize.Width / 2) - (boxX / 2),
				(Screen.ScreenSize.Height / 2) - (boxY / 2),
				(Screen.ScreenSize.Width / 2) + (boxX / 2),
				(Screen.ScreenSize.Height / 2) + (boxY / 2) + 20.0f + Base.DefaultTextFormat.FontSize
			);
			TextOrigin = new RawVector2(BoxRect.Left + 20.0f, BoxRect.Top + 10.0f);

			YesRect = new RawRectangleF(
				BoxRect.Left + 20.0f,
				BoxRect.Bottom - 40.0f,
				BoxRect.Left + 70.0f,
				BoxRect.Bottom - 10.0f
			);

			NoRect = new RawRectangleF(
				BoxRect.Right - 70.0f,
				BoxRect.Bottom - 40.0f,
				BoxRect.Right - 20.0f,
				BoxRect.Bottom - 10.0f
			);
		}

		int flashCount;
		Stopwatch flashSw = new Stopwatch();
		private void Flash()
		{
			flashCount = 0;
			flashSw.Start();
		}

		public void PushKey(char Key)
		{
			if (Key == '\r') {
				answerWait.Release();
				Elements.Remove(this);
				this.Text.Dispose();
				this.BoxCenterFormat.Dispose();
			} else
				Flash();
		}

		public void RebaseDX()
		{
			this.Text.Dispose();
			this.BoxCenterFormat.Dispose();
			InitText();
		}

		public async Task<bool> WaitForAnswer()
		{
			await answerWait.WaitAsync();
			return OnYes;
		}

		public void Render()
		{
			if (flashSw.IsRunning) {
				if (flashSw.Elapsed.TotalMilliseconds > 80) {
					flashCount++;
					flashSw.Restart();
				}

				BoxNegative = flashCount % 2 == 0;

				if (flashCount == 5)
					flashSw.Stop();
			}

			Func<ConsoleColor, SolidColorBrush> fx = BoxNegative ? new Func<ConsoleColor, SolidColorBrush>(Base.Brushes.GetNegativeColor) : new Func<ConsoleColor, SolidColorBrush>(Base.Brushes.GetColor);

			var backgr = fx(Settings.Colors.ConfirmBox.Background);
			var border = fx(Settings.Colors.ConfirmBox.Border);
			var textColor = fx(Settings.Colors.ConfirmBox.Text);
			var yesBackgr = fx(OnYes ? Settings.Colors.ConfirmBox.YesButtonActiveBackground : Settings.Colors.ConfirmBox.YesButtonBackground);
			var yesBorder = fx(Settings.Colors.ConfirmBox.YesButtonBorder);
			var noBackgr = fx(OnYes ? Settings.Colors.ConfirmBox.NoButtonBackground : Settings.Colors.ConfirmBox.NoButtonActiveBackground);
			var noBorder = fx(Settings.Colors.ConfirmBox.NoButtonBorder);
			var yesTextColor = fx(Settings.Colors.ConfirmBox.YesButtonText);
			var noTextColor = fx(Settings.Colors.ConfirmBox.NoButtonText);

			Base.D2DRenderTarget.FillRectangle(BoxRect, backgr);
			Base.D2DRenderTarget.DrawRectangle(BoxRect, border, 2.0f);
			Base.D2DRenderTarget.DrawTextLayout(TextOrigin, Text, textColor);

			Base.D2DRenderTarget.FillRectangle(YesRect, yesBackgr);
			Base.D2DRenderTarget.DrawRectangle(YesRect, yesBorder, 2.0f);

			Base.D2DRenderTarget.FillRectangle(NoRect, noBackgr);
			Base.D2DRenderTarget.DrawRectangle(NoRect, noBorder, 2.0f);

			Base.D2DRenderTarget.DrawText("Yes", BoxCenterFormat, YesRect, yesTextColor);
			Base.D2DRenderTarget.DrawText("No", BoxCenterFormat, NoRect, noTextColor);
		}

		public void SpecialKey(VirtualKeys.VK Key)
		{
			if (Key == VirtualKeys.VK.VK_RIGHT && OnYes)
				OnYes = false;
			else if (Key == VirtualKeys.VK.VK_LEFT && !OnYes)
				OnYes = true;
			else
				Flash();
		}

		~ConfirmBoxOverlay()
		{
			this.Text.Dispose();
			this.BoxCenterFormat.Dispose();
		}
	}
}
