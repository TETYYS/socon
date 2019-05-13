using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands.CP
{
	class LongCommand : ILongExecutableCommand
	{
		public TypeCode[][] ArgTypes => new TypeCode[][] { new TypeCode[] { } };

		public string Usage => "hmm";

		public async Task Execute(dynamic[] Args, CancellationToken CancelToken)
		{
			await Task.Delay(15000, CancelToken);
		}
	}
}
