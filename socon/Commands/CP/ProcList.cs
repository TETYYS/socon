using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using static socon.Native.soconwnt;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace socon.Commands.CP
{

	public class Win32_Process
	{
		public string Caption;
		public string CommandLine;
		public string CreationClassName;
		public string CreationDate; // DateTime
		public string CSCreationClassName;
		public string CSName;
		public string Description;
		public string ExecutablePath;
		public ushort? ExecutionState;
		public string Handle;
		public uint? HandleCount;
		public string InstallDate; // DateTime
		public ulong? KernelModeTime;
		public uint? MaximumWorkingSetSize;
		public uint? MinimumWorkingSetSize;
		public string Name;
		public string OSCreationClassName;
		public string OSName;
		public ulong? OtherOperationCount;
		public ulong? OtherTransferCount;
		public uint? PageFaults;
		public uint? PageFileUsage;
		public uint? ParentProcessId;
		public uint? PeakPageFileUsage;
		public ulong? PeakVirtualSize;
		public uint? PeakWorkingSetSize;
		public uint? Priority;
		public ulong? PrivatePageCount;
		public uint? ProcessId;
		public uint? QuotaNonPagedPoolUsage;
		public uint? QuotaPagedPoolUsage;
		public uint? QuotaPeakNonPagedPoolUsage;
		public uint? QuotaPeakPagedPoolUsage;
		public ulong? ReadOperationCount;
		public ulong? ReadTransferCount;
		public uint? SessionId;
		public string Status;
		public string TerminationDate; // DateTime
		public uint? ThreadCount;
		public ulong? UserModeTime;
		public ulong? VirtualSize;
		public string WindowsVersion;
		public ulong? WorkingSetSize;
		public ulong? WriteOperationCount;
		public ulong? WriteTransferCount;
	}

	class ProcList : ILongExecutableCommand
	{
		public ProcList()
		{
			ArgTypes = new TypeCode[][] { new[] { TypeCode.String }, new TypeCode[] { } };
		}
		public string Usage => "";

		public TypeCode[][] ArgTypes { get; }

		public async Task Execute(dynamic[] Args, CancellationToken Cancel)
		{
			bool explorerOnly = false;
			string explorerPath = "explorer.exe";

			if (Args.Length == 1 && Args[0].ToLower() == "e") {
				explorerOnly = true;

				try {
					var subKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", false);
					explorerPath = (string)subKey.GetValue("Shell");
				} catch { }
			}

			List<Win32_Process> procs = null;
			try {
				procs = await WMI.Utils.MapWMIToStructArray<Win32_Process>("SELECT * FROM Win32_Process", Cancel);
			} catch (Exception ex) {
				if (Cancel.IsCancellationRequested) {
					Render.DefaultSource.Instance.PushTextError("Process listing canceled");
					return;
				}

				throw ex;
			}

			IEnumerable<Win32_Process> baseProcesses;
			if (!explorerOnly) {
				baseProcesses = procs.Where(x => x.ParentProcessId == 0 || procs.Count(y => y.ProcessId == x.ParentProcessId) == 0);
			} else {
				var explorer = procs.FirstOrDefault(x => x.CommandLine == explorerPath);
				if (explorer == null)
					explorer = procs.FirstOrDefault(x => x.CommandLine == "explorer.exe");
				if (explorer != null)
					baseProcesses = procs.Where(x => x.ParentProcessId == explorer.ProcessId);
				else {
					Render.DefaultSource.Instance.PushTextError("Failed to find explorer");
					return;
				}
			}

			void printChildProcesses(Win32_Process proc, int nestLevel = 1)
			{
				var children = procs.Where(x => x.ParentProcessId == proc.ProcessId).ToList();
				for (int x = 0;x < children.Count;x++) {
					PROCESS_TOKEN_INTEGRITY integrity;
					ConsoleColor color;
					try {
						integrity = GetProcessIntegrity(children[x].ProcessId.Value);

						switch (integrity) {
							case PROCESS_TOKEN_INTEGRITY.UNTRUSTED:
								color = Settings.Colors.CPProcList.ProcessUntrusted;
								break;
							case PROCESS_TOKEN_INTEGRITY.LOW:
								color = Settings.Colors.CPProcList.ProcessLow;
								break;
							case PROCESS_TOKEN_INTEGRITY.MEDIUM:
								color = Settings.Colors.CPProcList.ProcessMedium;
								break;
							case PROCESS_TOKEN_INTEGRITY.MEDIUM_PLUS:
								color = Settings.Colors.CPProcList.ProcessMediumPlus;
								break;
							case PROCESS_TOKEN_INTEGRITY.HIGH:
								color = Settings.Colors.CPProcList.ProcessHigh;
								break;
							case PROCESS_TOKEN_INTEGRITY.SYSTEM:
								color = Settings.Colors.CPProcList.ProcessSystem;
								break;
							case PROCESS_TOKEN_INTEGRITY.PROTECTED:
								color = Settings.Colors.CPProcList.ProcessProtected;
								break;
							default:
								color = ConsoleColor.DarkMagenta;
								break;
						}
					} catch (Exception ex) {
						Debug.WriteLine(ex.Message);
						color = Settings.Colors.CPProcList.ProcessUnknown;
					}

					Render.DefaultSource.Instance.PushText(
						new String('\t', nestLevel - 1) +
						"➜ [" + children[x].ProcessId + "] " + children[x].Name,
						color
					);
					if (children[x].ProcessId != 0)
						printChildProcesses(children[x], nestLevel + 1);
				}
			}

			foreach (var proc in baseProcesses) {
				printChildProcesses(proc);
			}
		}
	}
}
