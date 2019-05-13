using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Linq;

namespace socon.Render {
	class TopLeftElements : IRenderer
	{
		public interface ITopLeftElementRenderer : IRenderer
		{
			SolidColorBrush Color { get; }
			RawVector2 Size { get; }
			bool RecalculateRect { get; set; }

			RectangleGeometry GeneratedRect { get; set; }
		}

		public void RebaseDX()
		{
			lock (Renderers) {
				foreach (var renderer in Renderers)
					renderer.RebaseDX();
			}
		}

		~TopLeftElements()
		{
			lock (Renderers)
				Renderers.Clear();
		}

		private List<ITopLeftElementRenderer> Renderers = new List<ITopLeftElementRenderer>();

		public void AddElement(ITopLeftElementRenderer Renderer)
		{
			int lastX = 0;
			lock (Renderers) {
				if (Renderers.Count != 0)
					lastX = Screen.ScreenSize.Width - (int)Renderers.Last().GeneratedRect.Rectangle.Left;

				var rectangleGeometry = new RectangleGeometry(Base.D2DFactory, new RawRectangleF(Screen.ScreenSize.Width - (lastX + Renderer.Size.X), 0, Screen.ScreenSize.Width - lastX, Renderer.Size.Y));
				Renderer.GeneratedRect = rectangleGeometry;

				Renderers.Add(Renderer);
			}
		}

		public void Render()
		{
			bool recalc = false;
			lock (Renderers) {
				for (int x = 0;x < Renderers.Count;x++) {
					var el = Renderers[x];

					if (el.RecalculateRect) {
						recalc = true;
						el.RecalculateRect = false;
					}
				
					if (recalc) {
						var prevX = Screen.ScreenSize.Width - (x == 0 ? 0 : (int)Renderers[x - 1].GeneratedRect.Rectangle.Left);
						el.GeneratedRect.Dispose();

						el.GeneratedRect = new RectangleGeometry(Base.D2DFactory, new RawRectangleF(Screen.ScreenSize.Width - (prevX + el.Size.X), 0, Screen.ScreenSize.Width - prevX, el.Size.Y));
					}

					if (el.Color != null)
						Base.D2DRenderTarget.DrawGeometry(el.GeneratedRect, el.Color);
					el.Render();
				}
			}
		}
	}
}
