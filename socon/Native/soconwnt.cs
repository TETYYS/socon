using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static socon.Native.WinAPI;

namespace socon.Native
{
	static class soconwnt
	{
		[DllImport("soconwnt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true, EntryPoint = "GetProcessIntegrity")]
		private static extern sbyte _GetProcessIntegrity(uint ProcessId);

		[DllImport("soconwnt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		private static extern string GetProcessImageName(uint ProcessId);

		[DllImport("soconwnt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		private static extern void FreeProcessImageName(string ImageName);
		
		[DllImport("soconwnt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true, EntryPoint = "EnablePrivileges")]
		private static extern bool _EnablePrivileges();

		[DllImport("soconwnt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern IntPtr ScwntCreateDesktop();

		public static bool PrivilegesEnabled { get; private set; }

		public enum PROCESS_TOKEN_INTEGRITY
		{
			UNTRUSTED,
			LOW,
			MEDIUM,
			MEDIUM_PLUS,
			HIGH,
			SYSTEM,
			PROTECTED
		}

		public static PROCESS_TOKEN_INTEGRITY GetProcessIntegrity(uint ProcessId)
		{
			sbyte ret = _GetProcessIntegrity(ProcessId);
			if (ret < 0)
				throw new Exception("Failed to query process integrity (" + ret + ")");

			switch (ret) {
				case 0:
					return PROCESS_TOKEN_INTEGRITY.UNTRUSTED;
				case 1:
					return PROCESS_TOKEN_INTEGRITY.LOW;
				case 2:
					return PROCESS_TOKEN_INTEGRITY.MEDIUM;
				case 3:
					return PROCESS_TOKEN_INTEGRITY.MEDIUM_PLUS;
				case 4:
					return PROCESS_TOKEN_INTEGRITY.HIGH;
				case 5:
					return PROCESS_TOKEN_INTEGRITY.SYSTEM;
				case 6:
					return PROCESS_TOKEN_INTEGRITY.PROTECTED;
				default:
					throw new Exception("Unknown error (665946)");
			}
		}

		public static bool EnablePrivileges()
		{
			return (PrivilegesEnabled = _EnablePrivileges());
		}
	}
}