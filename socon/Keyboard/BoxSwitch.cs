using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Keyboard
{
	static class BoxSwitch
	{
		private static int _stage;
		private static int Stage {
			get {
				return _stage;
			}
			set {
				_stage = value;
				Debug.WriteLine("Stage: " + value);
			}
		}
		public static void HandleKey(string Char, VirtualKeys.VK Key, bool Pressed)
		{
			Debug.Assert(!Base.TheBox);
			switch (Stage) {
				case 0:
				case 2:
					if (Key == VirtualKeys.VK.VK_LSHIFT && Pressed)
						Stage++;
					else Stage = 0;
					break;
				case 1:
				case 3:
					if (Key == VirtualKeys.VK.VK_LSHIFT && !Pressed)
						Stage++;
					else Stage = 0;
					break;
				case 4:
					if (Key == VirtualKeys.VK.VK_ADD && Pressed)
						Stage++;
					else Stage = 0;
					break;
				case 5:
					if (Char.Length == 1 && Char[0] == 's' && Pressed)
						Stage++;
					else Stage = 0;
					break;
				case 6:
				case 7:
					if ((Key == VirtualKeys.VK.VK_ADD || (Char.Length == 1 && Char[0] == 's')) && !Pressed)
						Stage++;
					else Stage = 0;
					break;
				case 8:
					if (Char.Length == 1 && Char[0] == 'c' && Pressed)
						Stage++;
					else Stage = 0;
					break;
				case 9:
					if (Char.Length == 1 && Char[0] == 'c' && !Pressed) {
						Base.InitShow();
						Stage = 0;
					}
					break;
			}
		}
	}
}
