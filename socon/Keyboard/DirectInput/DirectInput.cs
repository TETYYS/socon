using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;
using static socon.Keyboard.VirtualKeys;
using System.Diagnostics;
using System.Threading;

namespace socon.Keyboard
{
	class DirectInput : IKeyboardInput
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
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

		public string InputName => "DirectInput";

		public TimeSpan PressedInHoldTime { get; set; }
		public TimeSpan PressedInInterval { get; set; }

		public IKeyboardInputReceiver CurrentReceiver { get; private set; }

		public IKeyboardInputReceiver SwitchReceiver(IKeyboardInputReceiver recv)
		{
			var old = CurrentReceiver;
			CurrentReceiver = recv;
			return old;
		}

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

			var directInput = new SharpDX.DirectInput.DirectInput();

			var keybdGuid = Guid.Empty;

			foreach (var deviceInstance in directInput.GetDevices(DeviceType.Keyboard, DeviceEnumerationFlags.AllDevices))
				keybdGuid = deviceInstance.InstanceGuid;

			if (keybdGuid == Guid.Empty)
				Environment.Exit(1);

			var keybd = new SharpDX.DirectInput.Keyboard(directInput);

			keybd.Properties.BufferSize = 128;
			keybd.Acquire();

			string keys = "";
			VK vk = 0x00;
			KeyboardUpdate Scan = new KeyboardUpdate();
			Stopwatch holdInSW = new Stopwatch();
			Stopwatch holdInIntervalSW = new Stopwatch();
			holdInSW.Start();
			holdInIntervalSW.Start();

			var dwThread = Native.WinAPI.GetCurrentThreadId();
			while (true) {
				Thread.Sleep(1);
				Native.DesktopSwitch.PollAutoDesktopThreadSwitch(dwThread);
				keybd.Poll();
				var datas = keybd.GetBufferedData();

				if (datas.Length != 0)
					holdInSW.Restart();
				else if (holdInSW.Elapsed > PressedInHoldTime && Scan.IsPressed) {
					if (holdInIntervalSW.Elapsed > PressedInInterval) {
						HandleKey(keys, vk, Scan.Key, Scan.IsPressed);
						holdInIntervalSW.Restart();
					}
				}

				foreach (var state in datas) {
					/*if (state.Key == Key.F12) {
						Hooks.RemoveHooks();
						Environment.Exit(0);
						Debug.Assert(false);
						var a = (1 + 9) - 10;
						Debug.WriteLine((15 / a));
						return;
					}*/
					if (state.IsPressed) {
						switch (state.Key) {
							case Key.Capital:
									CapsLock = !CapsLock;
								break;
							case Key.NumberLock:
									NumLock = !NumLock;
								break;
							case Key.ScrollLock:
									ScrollLock = !ScrollLock;
								break;
						}
					}
					switch (state.Key) {
						case Key.RightShift:
							RShift = state.IsPressed;
							break;
						case Key.LeftShift:
							LShift = state.IsPressed;
							break;
						case Key.LeftControl:
							LCtrl = state.IsPressed;
							break;
						case Key.RightControl:
							RCtrl = state.IsPressed;
							break;
						case Key.LeftAlt:
							LAlt = state.IsPressed;
							break;
						case Key.RightAlt:
							RAlt = state.IsPressed;
							break;
					}

					Shift = LShift || RShift;
					Ctrl = LCtrl || RCtrl;
					Alt = LAlt || RAlt;

					byte[] keyState = new byte[256];

					if (CapsLock) keyState[(int)VK.VK_CAPITAL] = 0x01;
					if (NumLock) keyState[(int)VK.VK_NUMLOCK] = 0x01;
					if (ScrollLock) keyState[(int)VK.VK_SCROLL] = 0x01;
					if (Shift) keyState[(int)VK.VK_SHIFT] = 0x80;
					if (Ctrl) keyState[(int)VK.VK_CONTROL] = 0x80;
					if (Alt) keyState[(int)VK.VK_MENU] = 0x80;

					keys = ScancodeToUnicode(state.Key, keyState);
					vk = ScancodeToVKCode(state.Key);
					Scan = state;

					HandleKey(keys, vk, Scan.Key, Scan.IsPressed);
				}
			}
		}

		private void HandleKey(string Keys, VK VirtualKey, Key Scancode, bool Pressed)
		{
			if (!Base.TheBox && Keys.Length < 2) {
				BoxSwitch.HandleKey(Keys, VirtualKey, Pressed);
				return;
			}

			if (!Pressed)
				return;

			var special = new Key[] {
				Key.PageUp, Key.PageDown, Key.Home, Key.End, Key.Insert, Key.Delete,
				Key.Back,
				Key.Left, Key.Right, Key.Up, Key.Down,
				Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
				Key.Escape
			};

			if (special.Contains(Scancode)) {
				CurrentReceiver.SpecialKey(VirtualKey);
				return;
			}

			foreach (var key in Keys)
				CurrentReceiver.PushKey(key);
		}
	}
}
