using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon
{
	static class Screen
	{
		static Screen()
		{
			ScreenSize = new Size((int)System.Windows.SystemParameters.PrimaryScreenWidth, (int)System.Windows.SystemParameters.PrimaryScreenHeight);
			ScreenRect = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, ScreenSize.Width, ScreenSize.Height);
		}
		public static Size ScreenSize;
		public static SharpDX.Mathematics.Interop.RawRectangleF ScreenRect;
	}
}
