using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace socon.Commands.CP
{
	class Diag : IExecutableCommand
	{
		public Diag()
		{
			ArgTypes = new TypeCode[][] { new TypeCode[] { } };
		}
		public string Usage => "";

		public TypeCode[][] ArgTypes { get; }

		public async Task Execute(dynamic[] Args)
		{
			for (int x = 0;x < 1000;x++)
				Render.DefaultSource.Instance.PushTextNormal("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!@@@Ą");
		}
	}
}
