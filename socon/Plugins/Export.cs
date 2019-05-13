using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Plugins
{
	class Export : ISoconExports
	{
		public event ProcessCreatedCallback ProcessCreated {
			add {
				ETW.Listener.ProcessCreated += value;
			}
			remove {
				ETW.Listener.ProcessCreated -= value;
			}
		}

		public event ProcessExitedCallback ProcessExited {
			add {
				ETW.Listener.ProcessExited += value;
			}
			remove {
				ETW.Listener.ProcessExited -= value;
			}
		}

		public IntPtr ImportantMessageAdd(string Text)
		{
			return Render.ImportantMessage.Instance.AddMessage(Text);
		}

		public void ImportantMessageAddTimeout(string Text, TimeSpan Timeout)
		{
			Render.ImportantMessage.Instance.AddMessageTimeout(Text, Timeout);
		}

		public void ImportantMessageRemove(IntPtr Handle)
		{
			Render.ImportantMessage.Instance.RemoveMessage(Handle);
		}

		public void PushText(string Text, ConsoleColor Color)
		{
			Render.DefaultSource.Instance.PushText(Text, Color);
		}

		public void PushTextError(string Text)
		{
			Render.DefaultSource.Instance.PushTextError(Text);
		}

		public void PushTextNormal(string Text)
		{
			Render.DefaultSource.Instance.PushTextNormal(Text);
		}
	}
}
