using JsonConfig;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace socon.Commands
{
	static class Commands
	{
		public class Command
		{
			public string FullName;
			public ICommand IFace;

			public Command(Type Fx)
			{
				Debug.Assert(Fx.GetInterfaces().Contains(typeof(IExecutableCommand)) || Fx.GetInterfaces().Contains(typeof(ILongExecutableCommand)));
				IFace = (ICommand)Activator.CreateInstance(Fx);
				FullName = Fx.Name;
			}
		}

		public static void PopulateCommands()
		{
			var FullNames = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && x.Namespace == "socon.Commands.CP").Select(x => x).Where(x => x.Attributes == TypeAttributes.BeforeFieldInit);

			Debug.Assert(!FullNames.Select(x => x.Name).Contains("Alias"));

			ConfigObject commandsList = null;
			if (!(Settings.Global?.Commands is null) && Settings.GlobalNonDynamic != null)
				commandsList = ((ConfigObject)Settings.GlobalNonDynamic["Commands"]);

			FullNames.ToList().ForEach(x => AllCommands.Add(new Command(x)));

			/*foreach (var c in FullNames) {
				Command toAdd = new Command(c);
				if (commandsList != null && commandsList.ContainsKey(c)) {
					dynamic properties = ((dynamic)commandsList[c]);

					try {
						if (!(properties.Regex as string is null))
							toAdd.Regex = new Regex(properties.Regex, RegexOptions.Compiled);
					} catch (Exception ex) {
						Debug.WriteLine("Failed to compile regex (1):\n" + ex);
					}

					try {
						if (!(properties.UsageTrigger as string is null))
							toAdd.UsageTrigger = new Regex(properties.UsageTrigger, RegexOptions.Compiled);
					} catch (Exception ex) {
						Debug.WriteLine("Failed to compile regex (2):\n" + ex);
					}

					toAdd.Usage = properties.Usage as string ?? "";
				}
				AllCommands.Add(toAdd);
			}*/

			string key = "";
			foreach (var kv in ((dynamic)commandsList)?.Alias) {
				if (key == "") {
					key = kv;
				} else {
					try {
						Aliases.Add(new Regex(key, RegexOptions.Compiled), kv);
					} catch (Exception ex) {
						Debug.WriteLine("Failed to compile regex (3):\n" + ex);
					}
					key = "";
				}
			}
		}

		public static Dictionary<Regex, string> Aliases = new Dictionary<Regex, string>();
		public static List<Command> AllCommands = new List<Command>();
	}
}
