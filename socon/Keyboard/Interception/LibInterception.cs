using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace socon.Keyboard.Interception
{
	using InterceptionContext = IntPtr;
	using InterceptionDevice = Int32;
	using InterceptionPrecedence = Int32;
	using InterceptionFilter = UInt16;

	public static class Lib
	{

		public const int INTERCEPTION_MAX_KEYBOARD = 10;

		public const int INTERCEPTION_MAX_MOUSE = 10;

		public const int INTERCEPTION_MAX_DEVICE = ((INTERCEPTION_MAX_KEYBOARD) + (INTERCEPTION_MAX_MOUSE));

		public static int INTERCEPTION_KEYBOARD(int index) => ((index) + 1);

		public static int INTERCEPTION_MOUSE(int index) => ((INTERCEPTION_MAX_KEYBOARD) + (index) + 1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int InterceptionPredicate(InterceptionDevice device);

		[Flags]
		public enum InterceptionKeyState : ushort
		{
			INTERCEPTION_KEY_DOWN             = 0x00,
			INTERCEPTION_KEY_UP               = 0x01,
			INTERCEPTION_KEY_E0               = 0x02,
			INTERCEPTION_KEY_E1               = 0x04,
			INTERCEPTION_KEY_TERMSRV_SET_LED  = 0x08,
			INTERCEPTION_KEY_TERMSRV_SHADOW   = 0x10,
			INTERCEPTION_KEY_TERMSRV_VKPACKET = 0x20
		};

		public enum InterceptionFilterKeyState : ushort
		{
			INTERCEPTION_FILTER_KEY_NONE             = 0x0000,
			INTERCEPTION_FILTER_KEY_ALL              = 0xFFFF,
			INTERCEPTION_FILTER_KEY_DOWN             = InterceptionKeyState.INTERCEPTION_KEY_UP,
			INTERCEPTION_FILTER_KEY_UP               = InterceptionKeyState.INTERCEPTION_KEY_UP << 1,
			INTERCEPTION_FILTER_KEY_E0               = InterceptionKeyState.INTERCEPTION_KEY_E0 << 1,
			INTERCEPTION_FILTER_KEY_E1               = InterceptionKeyState.INTERCEPTION_KEY_E1 << 1,
			INTERCEPTION_FILTER_KEY_TERMSRV_SET_LED  = InterceptionKeyState.INTERCEPTION_KEY_TERMSRV_SET_LED << 1,
			INTERCEPTION_FILTER_KEY_TERMSRV_SHADOW   = InterceptionKeyState.INTERCEPTION_KEY_TERMSRV_SHADOW << 1,
			INTERCEPTION_FILTER_KEY_TERMSRV_VKPACKET = InterceptionKeyState.INTERCEPTION_KEY_TERMSRV_VKPACKET << 1
		};

		public enum InterceptionMouseState : ushort
		{
			INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN   = 0x001,
			INTERCEPTION_MOUSE_LEFT_BUTTON_UP     = 0x002,
			INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN  = 0x004,
			INTERCEPTION_MOUSE_RIGHT_BUTTON_UP    = 0x008,
			INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN = 0x010,
			INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP   = 0x020,

			INTERCEPTION_MOUSE_BUTTON_1_DOWN      = INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN,
			INTERCEPTION_MOUSE_BUTTON_1_UP        = INTERCEPTION_MOUSE_LEFT_BUTTON_UP,
			INTERCEPTION_MOUSE_BUTTON_2_DOWN      = INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN,
			INTERCEPTION_MOUSE_BUTTON_2_UP        = INTERCEPTION_MOUSE_RIGHT_BUTTON_UP,
			INTERCEPTION_MOUSE_BUTTON_3_DOWN      = INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN,
			INTERCEPTION_MOUSE_BUTTON_3_UP        = INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP,

			INTERCEPTION_MOUSE_BUTTON_4_DOWN      = 0x040,
			INTERCEPTION_MOUSE_BUTTON_4_UP        = 0x080,
			INTERCEPTION_MOUSE_BUTTON_5_DOWN      = 0x100,
			INTERCEPTION_MOUSE_BUTTON_5_UP        = 0x200,

			INTERCEPTION_MOUSE_WHEEL              = 0x400,
			INTERCEPTION_MOUSE_HWHEEL             = 0x800
		};

		public enum InterceptionFilterMouseState : ushort
		{
			INTERCEPTION_FILTER_MOUSE_NONE               = 0x0000,
			INTERCEPTION_FILTER_MOUSE_ALL                = 0xFFFF,

			INTERCEPTION_FILTER_MOUSE_LEFT_BUTTON_DOWN   = InterceptionMouseState.INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN,
			INTERCEPTION_FILTER_MOUSE_LEFT_BUTTON_UP     = InterceptionMouseState.INTERCEPTION_MOUSE_LEFT_BUTTON_UP,
			INTERCEPTION_FILTER_MOUSE_RIGHT_BUTTON_DOWN  = InterceptionMouseState.INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN,
			INTERCEPTION_FILTER_MOUSE_RIGHT_BUTTON_UP    = InterceptionMouseState.INTERCEPTION_MOUSE_RIGHT_BUTTON_UP,
			INTERCEPTION_FILTER_MOUSE_MIDDLE_BUTTON_DOWN = InterceptionMouseState.INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN,
			INTERCEPTION_FILTER_MOUSE_MIDDLE_BUTTON_UP   = InterceptionMouseState.INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP,

			INTERCEPTION_FILTER_MOUSE_BUTTON_1_DOWN      = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_1_DOWN,
			INTERCEPTION_FILTER_MOUSE_BUTTON_1_UP        = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_1_UP,
			INTERCEPTION_FILTER_MOUSE_BUTTON_2_DOWN      = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_2_DOWN,
			INTERCEPTION_FILTER_MOUSE_BUTTON_2_UP        = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_2_UP,
			INTERCEPTION_FILTER_MOUSE_BUTTON_3_DOWN      = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_3_DOWN,
			INTERCEPTION_FILTER_MOUSE_BUTTON_3_UP        = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_3_UP,

			INTERCEPTION_FILTER_MOUSE_BUTTON_4_DOWN      = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_4_DOWN,
			INTERCEPTION_FILTER_MOUSE_BUTTON_4_UP        = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_4_UP,
			INTERCEPTION_FILTER_MOUSE_BUTTON_5_DOWN      = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_5_DOWN,
			INTERCEPTION_FILTER_MOUSE_BUTTON_5_UP        = InterceptionMouseState.INTERCEPTION_MOUSE_BUTTON_5_UP,

			INTERCEPTION_FILTER_MOUSE_WHEEL              = InterceptionMouseState.INTERCEPTION_MOUSE_WHEEL,
			INTERCEPTION_FILTER_MOUSE_HWHEEL             = InterceptionMouseState.INTERCEPTION_MOUSE_HWHEEL,

			INTERCEPTION_FILTER_MOUSE_MOVE               = 0x1000
		}

		[Flags]
		public enum InterceptionMouseFlag : ushort
		{
			INTERCEPTION_MOUSE_MOVE_RELATIVE      = 0x000,
			INTERCEPTION_MOUSE_MOVE_ABSOLUTE      = 0x001,
			INTERCEPTION_MOUSE_VIRTUAL_DESKTOP    = 0x002,
			INTERCEPTION_MOUSE_ATTRIBUTES_CHANGED = 0x004,
			INTERCEPTION_MOUSE_MOVE_NOCOALESCE    = 0x008,
			INTERCEPTION_MOUSE_TERMSRV_SRC_SHADOW = 0x100
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct InterceptionMouseStroke
		{
			public InterceptionMouseState state;
			public InterceptionMouseFlag flags;
			public short rolling;
			public int x;
			public int y;
			public uint information;
		}

		[StructLayout(LayoutKind.Sequential, Size = 18)]
		public struct InterceptionKeyStroke
		{
			public ushort code;
			public InterceptionKeyState state;
			public uint information; // 8
		}

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern InterceptionContext interception_create_context();

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern void interception_destroy_context(InterceptionContext context);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern InterceptionPrecedence interception_get_precedence(InterceptionContext context, InterceptionDevice device);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern void interception_set_precedence(InterceptionContext context, InterceptionDevice device, InterceptionPrecedence precedence);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern InterceptionFilter interception_get_filter(InterceptionContext context, InterceptionDevice device);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		private static extern void interception_set_filter(InterceptionContext context, IntPtr predicate, InterceptionFilter filter);

		public static void interception_set_filter_mouse(InterceptionContext context, InterceptionFilterMouseState filter) =>
			interception_set_filter(context, Marshal.GetFunctionPointerForDelegate(new InterceptionPredicate(interception_is_mouse)), (InterceptionFilter)filter);

		public static void interception_set_filter_keyboard(InterceptionContext context, InterceptionFilterKeyState filter) =>
			interception_set_filter(context, Marshal.GetFunctionPointerForDelegate(new InterceptionPredicate(interception_is_keyboard)), (InterceptionFilter)filter);
		
		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern InterceptionDevice interception_wait(InterceptionContext context);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern InterceptionDevice interception_wait_with_timeout(InterceptionContext context, ulong milliseconds);

		[DllImport("interception.dll", CharSet = CharSet.Auto, EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_send_mouse(InterceptionContext context, InterceptionDevice device, [In] InterceptionMouseStroke[] stroke, uint nstroke);

		[DllImport("interception.dll", CharSet = CharSet.Auto, EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_send_keyboard(InterceptionContext context, InterceptionDevice device, [In] InterceptionKeyStroke[] stroke, uint nstroke);

		[DllImport("interception.dll", CharSet = CharSet.Auto, EntryPoint = "interception_receive", CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_receive_mouse(InterceptionContext context, InterceptionDevice device, [Out] InterceptionMouseStroke[] stroke, uint nstroke);

		[DllImport("interception.dll", CharSet = CharSet.Auto, EntryPoint = "interception_receive", CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_receive_keyboard(InterceptionContext context, InterceptionDevice device, [Out] InterceptionKeyStroke[] stroke, uint nstroke);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint interception_get_hardware_id(InterceptionContext context, InterceptionDevice device, IntPtr hardware_id_buffer, uint buffer_size);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_is_invalid(InterceptionDevice device);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_is_keyboard(InterceptionDevice device);

		[DllImport("interception.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
		public static extern int interception_is_mouse(InterceptionDevice device);
	}
}
