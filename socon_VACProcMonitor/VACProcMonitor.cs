using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using socon.Plugins;
using System.Diagnostics;

namespace socon_VACProcMonitor
{
	public class VACProcMonitor : ISoconPlugin
	{
		public string Name => "VAC process monitor";

		private IntPtr MsgHandle = IntPtr.Zero;

		public void Init()
		{
			Callbacks.ProcessCreated += this.ProcessCreated;
			Callbacks.ProcessExited += this.ProcessExited;

			if (Process.GetProcessesByName("ProcessHacker").Count() != 0 &&
				Process.GetProcessesByName("csgo").Count() != 0) {
				MsgHandle = Text.ImportantMessageAdd("VAC proc block");
			}
		}
		
		private void ProcessCreated(Microsoft.Diagnostics.Tracing.Parsers.Kernel.ProcessTraceData data)
		{
			if (data.ImageFileName.EndsWith("csgo.exe")) {
				var phs = Process.GetProcessesByName("ProcessHacker");
				if (phs.Count() != 0 && MsgHandle == IntPtr.Zero)
					MsgHandle = Text.ImportantMessageAdd("VAC proc block");
			}
		}

		private void ProcessExited(Microsoft.Diagnostics.Tracing.Parsers.Kernel.ProcessTraceData data)
		{
			if (data.ImageFileName.EndsWith("csgo.exe") || data.ImageFileName.EndsWith("ProcessHacker.exe")) {
				Text.ImportantMessageRemove(MsgHandle);
				MsgHandle = IntPtr.Zero;
			}
		}

		public void Unload()
		{
			Callbacks.ProcessCreated -= this.ProcessCreated;
			Callbacks.ProcessExited -= this.ProcessExited;
			if (MsgHandle != IntPtr.Zero)
				Text.ImportantMessageRemove(MsgHandle);
		}
	}
}
