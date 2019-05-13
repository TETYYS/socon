using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using socon.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.ETW
{
	public static class Listener
	{
		public static event ProcessCreatedCallback ProcessCreated;
		public static event ProcessExitedCallback ProcessExited;

		public static void Listen()
		{
			var session = new TraceEventSession(KernelTraceEventParser.KernelSessionName);
			session.StopOnDispose = true;
			session.EnableKernelProvider(KernelTraceEventParser.Keywords.ImageLoad | KernelTraceEventParser.Keywords.Process);

			session.Source.Kernel.ProcessStart += (ProcessTraceData data) => {
				ProcessCreated?.Invoke(data);
            };
			session.Source.Kernel.ProcessStop += (ProcessTraceData data) => {
				ProcessExited?.Invoke(data);
            };
			new Thread(() => session.Source.Process()).Start();
		}
	}
}
