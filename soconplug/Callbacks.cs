using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Plugins
{
	public static class Callbacks
	{
		public static event ProcessCreatedCallback ProcessCreated {
			add {
				PluginBase.Exports.ProcessCreated += value;
			}
			remove {
				PluginBase.Exports.ProcessCreated -= value;
			}
		}

		public static event ProcessExitedCallback ProcessExited {
			add {
				PluginBase.Exports.ProcessExited += value;
			}
			remove {
				PluginBase.Exports.ProcessExited -= value;
			}
		}
	}
}
