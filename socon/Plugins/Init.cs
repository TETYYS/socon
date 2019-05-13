using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace socon.Plugins
{
	class Init
	{
		public static void Load()
		{
			var pBaseAsm = Assembly.LoadFrom("soconplug.dll");

			foreach (var t in pBaseAsm.GetTypes()) {
				if (t == typeof(PluginBase)) {
					var pBase = Activator.CreateInstance(t) as PluginBase;
					Debug.Assert(pBase != null);
					pBase.Init(Assembly.GetExecutingAssembly());
				}
				/*if (t.GetInterface("IRTSharpImports") != null)
				{
					RTSharpPluginImports = Activator.CreateInstance(t) as IRTSharpImports;
				}*/
			}

			/*if (RTSharpPluginImports == null)
			{
				Logger.Log(LOG_LEVEL.FATAL, "Failed to import class from plugin base");
				Environment.Exit(0);
			}*/


			var asms = new List<Assembly>();
			foreach (var p in Directory.GetFiles(".", "socon_*.dll")) {
				Render.DefaultSource.Instance.PushTextNormal("Loading assembly " + p + "...");
				asms.Add(Assembly.LoadFrom(p));
			}

			foreach (var asm in asms) {
				foreach (var t in asm.GetTypes().Where(ft => ft.GetInterfaces().Contains(typeof(ISoconPlugin)))) {
					var plug = Activator.CreateInstance(t) as ISoconPlugin;
					Render.DefaultSource.Instance.PushTextNormal("Initializing " + plug.Name + "...");
					try {
						plug.Init();
					} catch (Exception ex) {
						Render.DefaultSource.Instance.PushTextError(plug.Name + " initialization failed");
						Render.DefaultSource.Instance.PushTextError(ex.ToString());
						continue;
					}
					Render.DefaultSource.Instance.PushTextCenter("Loaded " + plug.Name);
					Plugins.List.Add(plug);
				}
			}
		}
	}
}
