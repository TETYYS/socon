using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands.CP
{
	class Help : IExecutableCommand
	{
		public Help()
		{
			ArgTypes = new TypeCode[][] { new TypeCode[] { TypeCode.String }, new TypeCode[] { } };
		}
		public string Usage => "";

		public TypeCode[][] ArgTypes { get; }

		public async Task Execute(dynamic[] Args)
		{
			if (Args.Length == 0) {
				Render.DefaultSource.Instance.PushTextNormal(
					"Available commands:\n" + Commands.AllCommands.Select(x => x.FullName).Aggregate((i, j) => i + ", " + j)
				);
				return;
			} else {
				var cmd = Commands.AllCommands.Where(x => x.FullName == Args[0]).FirstOrDefault();
				if (cmd == null) {
					Render.DefaultSource.Instance.PushTextError("Command \"" + Args[0] + "\" not found");
					return;
				}

				Render.DefaultSource.Instance.PushTextNormal(cmd.IFace.Usage);
			}
		}
	}
}
