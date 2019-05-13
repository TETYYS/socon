using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using static socon.Render.TopLeftElements;
using System.Diagnostics;

namespace socon.Render
{
	class FPS : ITopLeftElementRenderer
	{
		RectangleGeometry _generatedRect;
		RawRectangleF TextRect;
		public RectangleGeometry GeneratedRect {
			get {
				return _generatedRect;
			}
			set {
				_generatedRect = value;
				TextRect = new RawRectangleF(
					value.Rectangle.Left + 1,
					value.Rectangle.Top + 1,
					value.Rectangle.Right - 1,
					value.Rectangle.Bottom - 1
				);
			}
		}

		private void Init()
		{
			Color = Base.Brushes.GetColor(Settings.Colors.PidBox);
			var text = new SharpDX.DirectWrite.TextLayout(Base.DWFactory, "999 FPS", Base.DefaultTextFormat, Screen.ScreenSize.Width, Screen.ScreenSize.Height);
			if (Math.Round(text.Metrics.Width) != text.Metrics.Width)
				Debug.WriteLine("Text metrics produce float values");
			Size = new RawVector2(text.Metrics.Width + 2, text.Metrics.Height + 2);
			text.Dispose();
		}

		public FPS()
		{
			Init();
		}

		public void RebaseDX()
		{
			Init();
			GeneratedRect = new RectangleGeometry(Base.D2DFactory, GeneratedRect.Rectangle);
		}

		~FPS()
		{
			GeneratedRect.Dispose();
		}

		public SolidColorBrush Color { get; set; }
		public RawVector2 Size { get; set; }

		public bool RecalculateRect { get; set; }

		public void Render()
		{
			var f = Math.Round(Base.FPS, 0);
			if (f > 999)
				f = 999;

			Base.D2DRenderTarget.DrawText(Math.Round(f, 0).ToString() + " FPS", Base.DefaultTextFormat, TextRect, Color, DrawTextOptions.Clip);
		}
	}
}
