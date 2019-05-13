using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands.CP
{
	class Exit : IExecutableCommand
	{
		public Exit()
		{
			ArgTypes = new TypeCode[][] { new TypeCode[] { }, new TypeCode[] { TypeCode.UInt64 } };
		}
		public string Usage => "";

		public TypeCode[][] ArgTypes { get; }

		public async Task Execute(dynamic[] Args)
		{
			if (Args.Length == 0 || (Args.Length == 1 && Args[0] == 0))
				Base.Exit();
			else if (Args.Length == 1 && Args[0] == 1) {
				var debug = new DeviceDebug(Base.DXDevice);
				debug.ReportLiveDeviceObjects(ReportingLevel.Detail | ReportingLevel.Summary);
				debug.Dispose();

				Base.Exit();
				Render.Elements.AllElements.Clear();
				Base.WaitForTheBox();

				Environment.Exit(0);
			}
		}
	}
}
