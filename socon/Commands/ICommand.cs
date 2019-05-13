using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands
{
	interface ILongExecutableCommand : ICommand
	{
		Task Execute(dynamic[] Args, CancellationToken CancelToken);
	}

	interface ICommand
	{
		TypeCode[][] ArgTypes { get; }
		string Usage { get; }
	}

	interface IExecutableCommand : ICommand
	{
		Task Execute(dynamic[] Args);
	}
}
