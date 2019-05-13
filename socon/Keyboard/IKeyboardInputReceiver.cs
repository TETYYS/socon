using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Keyboard
{
	public interface IKeyboardInputReceiver
	{
		void PushKey(char Key);
		void SpecialKey(VirtualKeys.VK Key);
	}
}
