using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Plugins
{
	public static class Text
	{
		public static void PushTextNormal(string Text)
		{
			PluginBase.Exports.PushTextNormal(Text);
		}

		public static void PushText(string Text, ConsoleColor Color)
		{
			PluginBase.Exports.PushText(Text, Color);
		}

		public static void PushTextError(string Text)
		{
			PluginBase.Exports.PushTextError(Text);
		}

		public static IntPtr ImportantMessageAdd(string Text)
		{
			return PluginBase.Exports.ImportantMessageAdd(Text);
		}

		public static void ImportantMessageAddTimeout(string Text, TimeSpan Timeout)
		{
			PluginBase.Exports.ImportantMessageAddTimeout(Text, Timeout);
		}

		public static void ImportantMessageRemove(IntPtr Handle)
		{
			PluginBase.Exports.ImportantMessageRemove(Handle);
		}
	}
}
