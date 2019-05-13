using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace socon.Plugins
{
	public interface ISoconExports
	{
		void PushTextNormal(string Text);

		void PushTextError(string Text);

		void PushText(string Text, ConsoleColor Color);

		event ProcessCreatedCallback ProcessCreated;

		event ProcessExitedCallback ProcessExited;

		IntPtr ImportantMessageAdd(string Text);

		void ImportantMessageAddTimeout(string Text, TimeSpan Timeout);

		void ImportantMessageRemove(IntPtr Handle);
	}

	public delegate void ProcessCreatedCallback(ProcessTraceData data);
	public delegate void ProcessExitedCallback(ProcessTraceData data);

	public class PluginBase
    {
		public static ISoconExports Exports;

		public void Init(Assembly socon)
		{
			bool initFound = false;
			foreach (Type t in socon.GetTypes())
			{
				if (t.GetInterface("ISoconExports") != null)
				{
					Exports = Activator.CreateInstance(t) as ISoconExports;
					initFound = true;
				}
			}

			Debug.Assert(initFound);
		}
	}
}