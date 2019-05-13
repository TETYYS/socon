using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands.CP
{
	class ProcKill : ILongExecutableCommand
	{
		public ProcKill()
		{
			ArgTypes = new TypeCode[][] { new TypeCode[] { TypeCode.String }, new TypeCode[] { TypeCode.UInt64 } };
		}
		public string Usage => "";

		public TypeCode[][] ArgTypes { get; }

		public async Task Execute(dynamic[] Args, CancellationToken Cancel)
		{
			string query;
			if (Args[0] is string)
				query = "SELECT Name, ProcessID FROM Win32_Process WHERE Name = '" + Args[0].Replace("'", "\\'") + "'";
			else
				query = "SELECT Name, ProcessID FROM Win32_Process WHERE ProcessID = " + Args[0];
			List<Win32_Process> procs = null;
			try {
				procs = await WMI.Utils.MapWMIToStructArray<Win32_Process>(query, Cancel);
			} catch (Exception ex) {
				if (Cancel.IsCancellationRequested) {
					Render.DefaultSource.Instance.PushTextError("Process listing canceled");
					return;
				}

				throw ex;
			}

			bool res = false;
			if (procs.Count == 0) {
				Render.DefaultSource.Instance.PushTextError("No processes found");
				return;
			} else if (procs.Count == 1)
				res = await Keyboard.ConfirmBox.Popup("Are you sure you want to terminate process \"" + procs[0].Name + "\" (" + procs[0].ProcessId + ")?");
			else if (procs.Count > 1) {
				res = await Keyboard.ConfirmBox.Popup("Are you sure you want to terminate following processes?\n" + String.Join("\n", procs.Select(x => "\"" + x.Name + "\" (" + x.ProcessId + ")")));
			}

			if (res) {
				foreach (var proc in procs) {
					Process.GetProcessById((int)proc.ProcessId.Value).Kill();
					Render.DefaultSource.Instance.PushTextNormal("Terminated " + proc.ProcessId);
				}
			}
		}
	}
}