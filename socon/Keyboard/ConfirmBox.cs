using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Keyboard
{
	public static class ConfirmBox
	{
		public static async Task<bool> Popup(string Text)
		{
			Debug.Assert(!Render.Elements.Exists<Render.ConfirmBoxOverlay>());
			var overlay = new Render.ConfirmBoxOverlay(Text);
			Render.Elements.Add(overlay);
			return await overlay.WaitForAnswer();
		}
	}
}
