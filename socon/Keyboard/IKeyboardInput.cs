using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Keyboard
{
	public interface IKeyboardInput
	{
		bool CapsLock { get; }
		bool NumLock { get; }
		bool ScrollLock { get; }
		bool Shift { get; }
		bool RShift { get; }
		bool LShift { get; }
		bool Ctrl { get; }
		bool LCtrl { get; }
		bool RCtrl { get; }
		bool Alt { get; }
		bool LAlt { get; }
		bool RAlt { get; }

		string InputName { get; }

		void ThreadStart();

		TimeSpan PressedInHoldTime { set; }

		TimeSpan PressedInInterval { set; }

		IKeyboardInputReceiver SwitchReceiver(IKeyboardInputReceiver recv);

		IKeyboardInputReceiver CurrentReceiver { get; }
	}
}
