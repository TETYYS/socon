using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace socon.Keyboard
{
	public static class VirtualKeys
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetKeyboardState(byte[] lpKeyState);

		[DllImport("user32.dll")]
		static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("user32.dll")]
		static extern IntPtr GetKeyboardLayout(uint idThread);

		const int MAPVK_VK_TO_CHAR = 2;
		const int MAPVK_VK_TO_VSC = 0;
		const int MAPVK_VSC_TO_VK = 1;
		const int MAPVK_VSC_TO_VK_EX = 3;

		public static string ScancodeToUnicode(uint ScanCode, byte[] KeyState, bool E0)
		{
			StringBuilder sbString = new StringBuilder(5);

			Debug.WriteLine(ScanCode.ToString("X2") + (E0 ? " E0!!!!!!!!!!!!!!!!!!!" : ""));

			if (E0) {
				var num = KeyState[(int)VK.VK_NUMLOCK] != 0x00;
				switch (ScanCode) {
					case 0x1d:
					case 0x5b:
					case 0x5c: /* other side, from the list */
					case 0x38:
					case 0x5d:
					case 0x53:
					case 0x37:
					case 0x2A:
					case 0x46:

					// http://www.computer-engineering.org/ps2keyboard/scancodes1.html
					case 0x5E:
					case 0x5F:
					case 0x63:
					case 0x19:
					case 0x10:
					case 0x24:
					case 0x22:
					case 0x20:
					case 0x30:
					case 0x2E:
					case 0x6D:
					case 0x6C:
					case 0x21:
					case 0x6B:
					case 0x65:
					case 0x32:
					case 0x6A:
					case 0x69:
					case 0x68:
					case 0x67:
					case 0x66:

						return "";
					case 0x52:
						if (num)
							return "0";
						return "";
					case 0x51:
						if (num)
							return "3";
						return "";
					case 0x50:
						if (num)
							return "2";
						return "";
					case 0x4f:
						if (num)
							return "1";
						return "";
					case 0x4d:
						if (num)
							return "6";
						return "";
					case 0x4b:
						if (num)
							return "4";
						return "";
					case 0x49:
						if (num)
							return "9";
						return "";
					case 0x48:
						if (num)
							return "8";
						return "";
					case 0x47:
						if (num)
							return "7";
						return "";
					case 0x1c:
						return "\n";
					case 0x35:
						return "/";
					default:
						break;
				}
			}

			uint vk = MapVirtualKey(ScanCode, MAPVK_VSC_TO_VK_EX);
			IntPtr HKL = GetKeyboardLayout(0);

			ToUnicodeEx(vk, (uint)ScanCode, KeyState, sbString, 5, 0, HKL);
			return sbString.ToString();
		}

		public static string ScancodeToUnicode(Key ScanCode, byte[] KeyState)
		{
			StringBuilder sbString = new StringBuilder(5);

			if (ScanCode == Key.NumberPadEnter)
				ScanCode = Key.Return;
			if (ScanCode == Key.Divide)
				ScanCode = Key.Slash;
			
			var numpadMap = new Key[] {
				Key.NumberPad0,
				Key.NumberPad1,
				Key.NumberPad2,
				Key.NumberPad3,
				Key.NumberPad4,
				Key.NumberPad5,
				Key.NumberPad6,
				Key.NumberPad7,
				Key.NumberPad8,
				Key.NumberPad9
			};
			if (KeyState[(int)VK.VK_NUMLOCK] == 0x01) {
				for (int x = 0;x < numpadMap.Length;x++) {
					if (numpadMap[x] == ScanCode)
						return x.ToString();
				}
			}

			uint vk = MapVirtualKey((uint)ScanCode, MAPVK_VSC_TO_VK_EX);
			IntPtr HKL = GetKeyboardLayout(0);

			ToUnicodeEx(vk, (uint)ScanCode, KeyState, sbString, 5, 0, HKL);
			return sbString.ToString();
		}

		static readonly Dictionary<uint, VK> E0Map = new Dictionary<uint, VK>() {
			{ 0x52, VK.VK_INSERT },
			{ 0x47, VK.VK_HOME },
			{ 0x49, VK.VK_PRIOR },
			//{ 0x37, VK. }
		};

		public static VK ScancodeToVKCode(uint Scancode, bool NumLock, bool Ctrl, bool Shift, bool E0, bool E1)
		{
			if (E0) {
				VK ret;
				if (E0Map.TryGetValue(Scancode, out ret))
					return ret;
			}
			return (VK)MapVirtualKey((uint)Scancode, MAPVK_VSC_TO_VK_EX);
		}

		/*public static VK ScancodeToVKCode(uint Scancode, bool NumLock, bool LCtrl, bool E0, bool E1)
		{
			if (E0) {
				switch (Scancode) {
					case 0x1d:
						return VK.VK_RCONTROL;
					case 0x5b:
						return VK.VK_LWIN;
					case 0x5c: /* ??? *
						return VK.VK_RWIN;
					case 0x38:
						return VK.VK_RMENU;
					case 0x5d:
						return VK.VK_APPS;
					case 0x53:
						/*if (NumLock)
							return VK.VK_DECIMAL;*
						return VK.VK_DELETE;
					case 0x37:
					case 0x2A:
						return VK.VK_SNAPSHOT;
					case 0x52:
						/*if (NumLock)
							return VK.VK_NUMPAD0;*
						return VK.VK_INSERT;
					case 0x51:
						/*if (NumLock)
							return VK.VK_NUMPAD3;*
						return VK.VK_NEXT;
					case 0x50:
						/*if (NumLock)
							return VK.VK_NUMPAD2;*
						return VK.VK_DOWN;
					case 0x4f:
						/*if (NumLock)
							return VK.VK_NUMPAD1;*
						return VK.VK_END;
					case 0x4d:
						/*if (NumLock)
							return VK.VK_NUMPAD6;*
						return VK.VK_RIGHT;
					case 0x4b:
						/*if (NumLock)
							return VK.VK_NUMPAD4;*
						return VK.VK_LEFT;
					case 0x49:
						/*if (NumLock)
							return VK.VK_NUMPAD9;*
						return VK.VK_PRIOR;
					case 0x48:
						/*if (NumLock)
							return VK.VK_NUMPAD8;*
						return VK.VK_UP;
					case 0x47:
						return VK.VK_HOME;
					case 0x1c:
						return VK.VK_RETURN;
					case 0x35:
						return VK.VK_DIVIDE;
					case 0x4c:
						return VK.VK_NUMPAD5;
					case 0x46:
						return VK.VK_CANCEL;

					// http://www.philipstorr.id.au/pcbook/book3/scancode.htm
					case 0x5E:
					case 0x5F:
					case 0x63:
						return (VK)0x07;
					case 0x19:
						return VK.VK_MEDIA_NEXT_TRACK;
					case 0x10:
						return VK.VK_MEDIA_PREV_TRACK;
					case 0x24:
						return VK.VK_MEDIA_STOP;
					case 0x22:
						return VK.VK_MEDIA_PLAY_PAUSE;
					case 0x20:
						return VK.VK_VOLUME_MUTE;
					case 0x30:
						return VK.VK_VOLUME_UP;
					case 0x2E:
						return VK.VK_VOLUME_DOWN;
					case 0x6D:
						return VK.VK_LAUNCH_MEDIA_SELECT;
					case 0x6C:
						return VK.VK_LAUNCH_MAIL;
					case 0x21:
						return VK.VK_LAUNCH_APP1;
					case 0x6B:
						return VK.VK_LAUNCH_APP2;
					case 0x65:
						return VK.VK_BROWSER_SEARCH;
					case 0x32:
						return VK.VK_BROWSER_HOME;
					case 0x6A:
						return VK.VK_BROWSER_BACK;
					case 0x69:
						return VK.VK_BROWSER_FORWARD;
					case 0x68:
						return VK.VK_BROWSER_STOP;
					case 0x67:
						return VK.VK_BROWSER_REFRESH;
					case 0x66:
						return VK.VK_BROWSER_FAVORITES;

					default:
						break;
				}
			} else if (NumLock) {
				switch (Scancode) {
					case 0x53:
						return VK.VK_DECIMAL;
					case 0x37:
					case 0x2A:
						return VK.VK_SNAPSHOT;
					case 0x52:
						return VK.VK_NUMPAD0;
					case 0x51:
						return VK.VK_NUMPAD3;
					case 0x50:
						return VK.VK_NUMPAD2;
					case 0x4f:
						return VK.VK_NUMPAD1;
					case 0x4d:
						return VK.VK_NUMPAD6;
					case 0x4b:
						return VK.VK_NUMPAD4;
					case 0x4c:
						return VK.VK_NUMPAD5;
					case 0x49:
						return VK.VK_NUMPAD9;
					case 0x48:
						return VK.VK_NUMPAD8;
					case 0x47:
						return VK.VK_NUMPAD7;
				}
			}
			if (E1) {
				switch (Scancode) {
					case 0x45:
						if (LCtrl)
							return VK.VK_PAUSE;
						break;
					case 0x1d:
						break;
					default:
						break;
				}
			}
			return (VK)MapVirtualKey((uint)Scancode, MAPVK_VSC_TO_VK_EX);
		}*/

		public static VK ScancodeToVKCode(Key Scancode)
		{
			switch (Scancode) {
				case Key.Left: return VK.VK_LEFT;
				case Key.Right: return VK.VK_RIGHT;
				case Key.Up: return VK.VK_UP;
				case Key.Down: return VK.VK_DOWN;
			}
			return (VK)MapVirtualKey((uint)Scancode, MAPVK_VSC_TO_VK_EX);
		}

		public static string VKCodeToUnicode(VK VKCode, byte[] KeyState)
		{
			StringBuilder sbString = new StringBuilder();
			
			uint scanCode = MapVirtualKey((uint)VKCode, MAPVK_VK_TO_VSC);
			IntPtr HKL = GetKeyboardLayout(0);

			ToUnicodeEx((uint)VKCode, scanCode, KeyState, sbString, 5, 0, HKL);
			return sbString.ToString();
		}

		public enum VK : uint
		{
			/*
			 * Virtual Keys, Standard Set
			 */
			VK_LBUTTON = 0x01,
			VK_RBUTTON = 0x02,
			VK_CANCEL = 0x03,
			VK_MBUTTON = 0x04,    /* NOT contiguous with L & RBUTTON */

			VK_XBUTTON1 = 0x05,    /* NOT contiguous with L & RBUTTON */
			VK_XBUTTON2 = 0x06,    /* NOT contiguous with L & RBUTTON */

			/*
			 * 0x07 : reserved
			 */


			VK_BACK = 0x08,
			VK_TAB = 0x09,

			/*
			 * 0x0A - 0x0B : reserved
			 */

			VK_CLEAR = 0x0C,
			VK_RETURN = 0x0D,

			/*
			 * 0x0E - 0x0F : unassigned
			 */

			VK_SHIFT = 0x10,
			VK_CONTROL = 0x11,
			VK_MENU = 0x12,
			VK_PAUSE = 0x13,
			VK_CAPITAL = 0x14,

			VK_KANA = 0x15,
			VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
			VK_HANGUL = 0x15,

			/*
			 * 0x16 : unassigned
			 */

			VK_JUNJA = 0x17,
			VK_FINAL = 0x18,
			VK_HANJA = 0x19,
			VK_KANJI = 0x19,

			/*
			 * 0x1A : unassigned
			 */

			VK_ESCAPE = 0x1B,

			VK_CONVERT = 0x1C,
			VK_NONCONVERT = 0x1D,
			VK_ACCEPT = 0x1E,
			VK_MODECHANGE = 0x1F,

			VK_SPACE = 0x20,
			VK_PRIOR = 0x21,
			VK_NEXT = 0x22,
			VK_END = 0x23,
			VK_HOME = 0x24,
			VK_LEFT = 0x25,
			VK_UP = 0x26,
			VK_RIGHT = 0x27,
			VK_DOWN = 0x28,
			VK_SELECT = 0x29,
			VK_PRINT = 0x2A,
			VK_EXECUTE = 0x2B,
			VK_SNAPSHOT = 0x2C,
			VK_INSERT = 0x2D,
			VK_DELETE = 0x2E,
			VK_HELP = 0x2F,

			/*
			 * VK_0 - VK_9 are the same as ASCII '0' - '9' (0x30 - 0x39)
			 * 0x3A - 0x40 : unassigned
			 * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A)
			 */

			VK_LWIN = 0x5B,
			VK_RWIN = 0x5C,
			VK_APPS = 0x5D,

			/*
			 * 0x5E : reserved
			 */

			VK_SLEEP = 0x5F,

			VK_NUMPAD0 = 0x60,
			VK_NUMPAD1 = 0x61,
			VK_NUMPAD2 = 0x62,
			VK_NUMPAD3 = 0x63,
			VK_NUMPAD4 = 0x64,
			VK_NUMPAD5 = 0x65,
			VK_NUMPAD6 = 0x66,
			VK_NUMPAD7 = 0x67,
			VK_NUMPAD8 = 0x68,
			VK_NUMPAD9 = 0x69,
			VK_MULTIPLY = 0x6A,
			VK_ADD = 0x6B,
			VK_SEPARATOR = 0x6C,
			VK_SUBTRACT = 0x6D,
			VK_DECIMAL = 0x6E,
			VK_DIVIDE = 0x6F,
			VK_F1 = 0x70,
			VK_F2 = 0x71,
			VK_F3 = 0x72,
			VK_F4 = 0x73,
			VK_F5 = 0x74,
			VK_F6 = 0x75,
			VK_F7 = 0x76,
			VK_F8 = 0x77,
			VK_F9 = 0x78,
			VK_F10 = 0x79,
			VK_F11 = 0x7A,
			VK_F12 = 0x7B,
			VK_F13 = 0x7C,
			VK_F14 = 0x7D,
			VK_F15 = 0x7E,
			VK_F16 = 0x7F,
			VK_F17 = 0x80,
			VK_F18 = 0x81,
			VK_F19 = 0x82,
			VK_F20 = 0x83,
			VK_F21 = 0x84,
			VK_F22 = 0x85,
			VK_F23 = 0x86,
			VK_F24 = 0x87,

			/*
			 * 0x88 - 0x8F : UI navigation
			 */

			VK_NAVIGATION_VIEW = 0x88, // reserved
			VK_NAVIGATION_MENU = 0x89, // reserved
			VK_NAVIGATION_UP = 0x8A, // reserved
			VK_NAVIGATION_DOWN = 0x8B, // reserved
			VK_NAVIGATION_LEFT = 0x8C, // reserved
			VK_NAVIGATION_RIGHT = 0x8D, // reserved
			VK_NAVIGATION_ACCEPT = 0x8E, // reserved
			VK_NAVIGATION_CANCEL = 0x8F, // reserved

			VK_NUMLOCK = 0x90,
			VK_SCROLL = 0x91,

			/*
			 * NEC PC-9800 kbd definitions
			 */
			VK_OEM_NEC_EQUAL = 0x92,   // '=' key on numpad

			/*
			 * Fujitsu/OASYS kbd definitions
			 */
			VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
			VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
			VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
			VK_OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
			VK_OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key

			/*
			 * 0x97 - 0x9F : unassigned
			 */

			/*
			 * VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys.
			 * Used only as parameters to GetAsyncKeyState() and GetKeyState().
			 * No other API or message will distinguish left and right keys in this way.
			 */
			VK_LSHIFT = 0xA0,
			VK_RSHIFT = 0xA1,
			VK_LCONTROL = 0xA2,
			VK_RCONTROL = 0xA3,
			VK_LMENU = 0xA4,
			VK_RMENU = 0xA5,

			VK_BROWSER_BACK = 0xA6,
			VK_BROWSER_FORWARD = 0xA7,
			VK_BROWSER_REFRESH = 0xA8,
			VK_BROWSER_STOP = 0xA9,
			VK_BROWSER_SEARCH = 0xAA,
			VK_BROWSER_FAVORITES = 0xAB,
			VK_BROWSER_HOME = 0xAC,

			VK_VOLUME_MUTE = 0xAD,
			VK_VOLUME_DOWN = 0xAE,
			VK_VOLUME_UP = 0xAF,
			VK_MEDIA_NEXT_TRACK = 0xB0,
			VK_MEDIA_PREV_TRACK = 0xB1,
			VK_MEDIA_STOP = 0xB2,
			VK_MEDIA_PLAY_PAUSE = 0xB3,
			VK_LAUNCH_MAIL = 0xB4,
			VK_LAUNCH_MEDIA_SELECT = 0xB5,
			VK_LAUNCH_APP1 = 0xB6,
			VK_LAUNCH_APP2 = 0xB7,


			/*
			 * 0xB8 - 0xB9 : reserved
			 */

			VK_OEM_1 = 0xBA,   // ';:' for US
			VK_OEM_PLUS = 0xBB,   // '+' any country
			VK_OEM_COMMA = 0xBC,   // ',' any country
			VK_OEM_MINUS = 0xBD,   // '-' any country
			VK_OEM_PERIOD = 0xBE,   // '.' any country
			VK_OEM_2 = 0xBF,   // '/?' for US
			VK_OEM_3 = 0xC0,   // '`~' for US

			/*
			 * 0xC1 - 0xC2 : reserved
			 */


			/*
			 * 0xC3 - 0xDA : Gamepad input
			 */

			VK_GAMEPAD_A = 0xC3, // reserved
			VK_GAMEPAD_B = 0xC4, // reserved
			VK_GAMEPAD_X = 0xC5, // reserved
			VK_GAMEPAD_Y = 0xC6, // reserved
			VK_GAMEPAD_RIGHT_SHOULDER = 0xC7, // reserved
			VK_GAMEPAD_LEFT_SHOULDER = 0xC8, // reserved
			VK_GAMEPAD_LEFT_TRIGGER = 0xC9, // reserved
			VK_GAMEPAD_RIGHT_TRIGGER = 0xCA, // reserved
			VK_GAMEPAD_DPAD_UP = 0xCB, // reserved
			VK_GAMEPAD_DPAD_DOWN = 0xCC, // reserved
			VK_GAMEPAD_DPAD_LEFT = 0xCD, // reserved
			VK_GAMEPAD_DPAD_RIGHT = 0xCE, // reserved
			VK_GAMEPAD_MENU = 0xCF, // reserved
			VK_GAMEPAD_VIEW = 0xD0, // reserved
			VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON = 0xD1, // reserved
			VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON = 0xD2, // reserved
			VK_GAMEPAD_LEFT_THUMBSTICK_UP = 0xD3, // reserved
			VK_GAMEPAD_LEFT_THUMBSTICK_DOWN = 0xD4, // reserved
			VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT = 0xD5, // reserved
			VK_GAMEPAD_LEFT_THUMBSTICK_LEFT = 0xD6, // reserved
			VK_GAMEPAD_RIGHT_THUMBSTICK_UP = 0xD7, // reserved
			VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN = 0xD8, // reserved
			VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT = 0xD9, // reserved
			VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT = 0xDA, // reserved



			VK_OEM_4 = 0xDB,  //  '[{' for US
			VK_OEM_5 = 0xDC,  //  '\|' for US
			VK_OEM_6 = 0xDD,  //  ']}' for US
			VK_OEM_7 = 0xDE,  //  ''"' for US
			VK_OEM_8 = 0xDF,

			/*
			 * 0xE0 : reserved
			 */

			/*
			 * Various extended or enhanced keyboards
			 */
			VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
			VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
			VK_ICO_HELP = 0xE3,  //  Help key on ICO
			VK_ICO_00 = 0xE4,  //  00 key on ICO

			VK_PROCESSKEY = 0xE5,

			VK_ICO_CLEAR = 0xE6,

			VK_PACKET = 0xE7,

			/*
			 * 0xE8 : unassigned
			 */

			/*
			 * Nokia/Ericsson definitions
			 */
			VK_OEM_RESET = 0xE9,
			VK_OEM_JUMP = 0xEA,
			VK_OEM_PA1 = 0xEB,
			VK_OEM_PA2 = 0xEC,
			VK_OEM_PA3 = 0xED,
			VK_OEM_WSCTRL = 0xEE,
			VK_OEM_CUSEL = 0xEF,
			VK_OEM_ATTN = 0xF0,
			VK_OEM_FINISH = 0xF1,
			VK_OEM_COPY = 0xF2,
			VK_OEM_AUTO = 0xF3,
			VK_OEM_ENLW = 0xF4,
			VK_OEM_BACKTAB = 0xF5,

			VK_ATTN = 0xF6,
			VK_CRSEL = 0xF7,
			VK_EXSEL = 0xF8,
			VK_EREOF = 0xF9,
			VK_PLAY = 0xFA,
			VK_ZOOM = 0xFB,
			VK_NONAME = 0xFC,
			VK_PA1 = 0xFD,
			VK_OEM_CLEAR = 0xFE,

			/*
			 * 0xFF : reserved
			 */
		}
	}
}
