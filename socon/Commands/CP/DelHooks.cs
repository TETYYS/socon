using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands.CP
{
	class DelHooks : IExecutableCommand
	{
		public DelHooks()
		{
			ArgTypes = new TypeCode[][] { new TypeCode[] { } };
		}
		public string Usage => "";

		public TypeCode[][] ArgTypes { get; }

		public async Task Execute(dynamic[] Args)
		{
			Hooks.RemoveHooks();
			Render.DefaultSource.Instance.PushTextNormal("Hooks removed");
		}
	}
}
