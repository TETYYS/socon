using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using static socon.Keyboard.VirtualKeys;
using System.Diagnostics;
using System.Threading;
using socon.Keyboard.Interception;
using socon.Native;

namespace socon.Keyboard
{
	using InterceptionContext = IntPtr;
	using InterceptionDevice = Int32;
	using InterceptionPrecedence = Int32;
	using InterceptionFilter = UInt16;

	class KeyboardFilter : IKeyboardInput
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool GetKeyboardState(byte[] lpKeyState);

		public bool CapsLock { get; set; }
		public bool NumLock { get; set; }
		public bool ScrollLock { get; set; }
		public bool Shift { get; set; }
		public bool RShift { get; set; }
		public bool LShift { get; set; }
		public bool Ctrl { get; set; }
		public bool LCtrl { get; set; }
		public bool RCtrl { get; set; }
		public bool Alt { get; set; }
		public bool LAlt { get; set; }
		public bool RAlt { get; set; }

		private bool E0;
		private bool E1;

		public string InputName => "KeyboardFilter";

		public TimeSpan PressedInHoldTime { get; set; }
		public TimeSpan PressedInInterval { get; set; }

		public IKeyboardInputReceiver CurrentReceiver { get; private set; }

		public IKeyboardInputReceiver SwitchReceiver(IKeyboardInputReceiver recv)
		{
			var old = CurrentReceiver;
			CurrentReceiver = recv;
			return old;
		}

		byte[] keyState = new byte[256];

		public void ThreadStart()
		{
			byte[] keybdState = new byte[256];
			GetKeyboardState(keybdState);

			NumLock = (keybdState[(int)VK.VK_NUMLOCK] & 1) != 0;
			CapsLock = (keybdState[(int)VK.VK_CAPITAL] & 1) != 0;
			ScrollLock = (keybdState[(int)VK.VK_SCROLL] & 1) != 0;

			Shift = (keybdState[(int)VK.VK_SHIFT] & 0x80) != 0;
			LShift = (keybdState[(int)VK.VK_LSHIFT] & 0x80) != 0;
			RShift = (keybdState[(int)VK.VK_RSHIFT] & 0x80) != 0;

			Ctrl = (keybdState[(int)VK.VK_CONTROL] & 0x80) != 0;
			LCtrl = (keybdState[(int)VK.VK_LCONTROL] & 0x80) != 0;
			RCtrl = (keybdState[(int)VK.VK_RCONTROL] & 0x80) != 0;

			Alt = (keybdState[(int)VK.VK_MENU] & 0x80) != 0;
			LAlt = (keybdState[(int)VK.VK_LMENU] & 0x80) != 0;
			RAlt = (keybdState[(int)VK.VK_RMENU] & 0x80) != 0;

			InterceptionContext context = IntPtr.Zero;
			InterceptionDevice device;

			WinAPI.SetThreadPriority(WinAPI.GetCurrentThread(), WinAPI.ThreadPriority.THREAD_PRIORITY_TIME_CRITICAL);

			context = Lib.interception_create_context();
			Console.WriteLine("Ctx: " + context);
			Lib.interception_set_filter_keyboard(context, Lib.InterceptionFilterKeyState.INTERCEPTION_FILTER_KEY_ALL);

			Lib.InterceptionKeyStroke[] rawKeys = new Lib.InterceptionKeyStroke[1];

			string keys = "";
			VK vk = 0x00;
			Stopwatch holdInSW = new Stopwatch();
			Stopwatch holdInIntervalSW = new Stopwatch();
			holdInSW.Start();
			holdInIntervalSW.Start();
			
			while (Lib.interception_receive_keyboard(context, device = Lib.interception_wait(context), rawKeys, 1) > 0) {
				var key = rawKeys.First();
				if (key.state.HasFlag(Lib.InterceptionKeyState.INTERCEPTION_KEY_UP) && key.code == 0x54) {
					if (!Base.TheBox)
						Base.InitShow();
					else
						Base.Exit();
					continue;
				}

				Lib.interception_send_keyboard(context, device, rawKeys, 1);
				
				holdInSW.Restart();

				var isPressed = !key.state.HasFlag(Lib.InterceptionKeyState.INTERCEPTION_KEY_UP);

				if (key.state.HasFlag(Lib.InterceptionKeyState.INTERCEPTION_KEY_E0))
					E0 = isPressed;
				if (key.state.HasFlag(Lib.InterceptionKeyState.INTERCEPTION_KEY_E1))
					E1 = isPressed;

				var scancode = key.code;
				
				if (holdInSW.Elapsed > PressedInHoldTime && isPressed) {
					if (holdInIntervalSW.Elapsed > PressedInInterval) {
						HandleKey(keys, vk, key.code, isPressed);
						holdInIntervalSW.Restart();
					}
				}

				if (isPressed) {
					switch (scancode) {
						case 58: CapsLock =		!CapsLock;		break;
						case 69: NumLock =		!NumLock;		break;
						case 70: ScrollLock =	!ScrollLock;	break;
					}
				}
				switch (scancode) {
					case 54:	RShift =	isPressed; break;
					case 42:	LShift =	isPressed; break;
					case 29:	LCtrl =		isPressed; break;
					case 157:	RCtrl =		isPressed; break;
					case 56:	LAlt =		isPressed; break;
					case 184:	RAlt =		isPressed; break;
				}

				Shift = LShift ||	RShift;
				Ctrl =	LCtrl ||	RCtrl;
				Alt =	LAlt ||		RAlt;

				if (CapsLock)	keyState[(int)VK.VK_CAPITAL	] = 0x01; else keyState[(int)VK.VK_CAPITAL	] = 0x00;
				if (NumLock)	keyState[(int)VK.VK_NUMLOCK	] = 0x01; else keyState[(int)VK.VK_NUMLOCK	] = 0x00;
				if (ScrollLock)	keyState[(int)VK.VK_SCROLL	] = 0x01; else keyState[(int)VK.VK_SCROLL	] = 0x00;
				if (Shift)		keyState[(int)VK.VK_SHIFT	] = 0x80; else keyState[(int)VK.VK_SHIFT	] = 0x00;
				if (Ctrl)		keyState[(int)VK.VK_CONTROL	] = 0x80; else keyState[(int)VK.VK_CONTROL	] = 0x00;
				if (Alt)		keyState[(int)VK.VK_MENU	] = 0x80; else keyState[(int)VK.VK_MENU		] = 0x00;

				keys = ScancodeToUnicode(scancode, keyState, E0);
				vk = ScancodeToVKCode(scancode, NumLock, Ctrl, Shift, E0, E1);

				HandleKey(keys, vk, scancode, isPressed);

				//Render.ImportantMessage.Instance.AddMessageTimeout("keys: " + keys + "VK: " + vk + " Scancode " + scancode + " pressed? " + (isPressed ? "true" : "false"), TimeSpan.FromSeconds(1));
			}

			Render.ImportantMessage.Instance.AddMessageTimeout("Interception lost device handle", TimeSpan.FromMinutes(1));
			Lib.interception_destroy_context(context);
		}

		readonly List<VK> special = new List<VK>(new[] {
			VK.VK_PRIOR /* PGUP */, VK.VK_NEXT /* PGDN */, VK.VK_HOME, VK.VK_END, VK.VK_INSERT, VK.VK_DELETE,
			VK.VK_BACK,
			VK.VK_LEFT, VK.VK_RIGHT, VK.VK_UP, VK.VK_DOWN,
			VK.VK_F1, VK.VK_F2, VK.VK_F3, VK.VK_F4, VK.VK_F5, VK.VK_F6, VK.VK_F7, VK.VK_F8, VK.VK_F9, VK.VK_F10, VK.VK_F11, VK.VK_F12,
			VK.VK_ESCAPE
		});

		private void HandleKey(string Keys, VK VirtualKey, int Scancode, bool Pressed)
		{
			if (!Base.TheBox && Keys.Length < 2) {
				BoxSwitch.HandleKey(Keys, VirtualKey, Pressed);
				return;
			}

			if (!Pressed)
				return;

			if (special.Contains(VirtualKey)) {
				CurrentReceiver.SpecialKey(VirtualKey);
				return;
			}

			foreach (var key in Keys)
				CurrentReceiver.PushKey(key);
		}
	}
}