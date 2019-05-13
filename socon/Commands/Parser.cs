using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon.Commands
{
	static class Parser
	{
		public static bool CommandExecuting { get; private set; }
		public static async Task ParseCommand(string Cmd)
		{
			foreach (var alias in Commands.Aliases) {
				if (alias.Key.IsMatch(Cmd)) {
					Cmd = alias.Key.Replace(Cmd, alias.Value);
					break;
				}
			}

			Commands.Command command = null;

			string cmdName = "";
			List<dynamic> args = new List<dynamic>();

			var argsStart = Cmd.IndexOf('(');
			if (argsStart == -1) {
				cmdName = Cmd;
			} else {
				cmdName = Cmd.Substring(0, argsStart);

				foreach (var cmd in Commands.AllCommands) {
					if (cmd.FullName == cmdName)
						command = cmd;
				}

				Cmd = Cmd.Remove(0, argsStart + 1);
				if (Cmd.EndsWith(")"))
					Cmd = Cmd.Remove(Cmd.Length - 1, 1);
				else {
					Render.DefaultSource.Instance.PushTextError("> Invalid arguments");
					return;
				}

				while (Cmd.Length != 0) {
					if (Cmd[0] == '"') {
						// String
						int stringEnd;
						if ((stringEnd = Cmd.IndexOf('"', 1)) == -1) {
							Render.DefaultSource.Instance.PushTextError("> Expected end of string");
							return;
						}

						args.Add(Cmd.Substring(1, stringEnd - 1));
						Cmd = Cmd.Remove(0, stringEnd + 1);
					} else if (Cmd[0] == '\'') {
						// Char
						if (Cmd.Length < 2 || Cmd[2] != '\'') {
							Render.DefaultSource.Instance.PushTextError("> Expected end of char");
							return;
						}

						args.Add(Cmd[1]);
						Cmd = Cmd.Remove(0, 3);
					} else if ((Cmd[0] >= '0' && Cmd[0] <= '9') || Cmd[0] == '-') {
						// Number

						int numEnd = Cmd[0] == '-' ? 1 : 0;
						while (numEnd < Cmd.Length && ((Cmd[numEnd] >= '0' && Cmd[numEnd] <= '9') || Cmd[numEnd] == '.'))
							numEnd++;

						var sNum = Cmd.Substring(0, numEnd);

						if (sNum.Replace(".", "").Length == sNum.Length - 1) {
							decimal num;
							if (!Decimal.TryParse(sNum, out num)) {
								Render.DefaultSource.Instance.PushTextError("> Unexpected floating point number failure");
								return;
							}

							args.Add(num);
						} else if (sNum[0] == '-') {
							long num;
							if (!Int64.TryParse(sNum, out num)) {
								Render.DefaultSource.Instance.PushTextError("> Unexpected negative number failure");
								return;
							}

							args.Add(num);
						} else {
							ulong num;
							if (!UInt64.TryParse(sNum, out num)) {
								Render.DefaultSource.Instance.PushTextError("> Unexpected number failure");
								return;
							}

							args.Add(num);
						}
						Cmd = Cmd.Remove(0, sNum.Length);
					} else if (Cmd.ToLower().StartsWith("true")) {
						Cmd = Cmd.Remove(0, 4);
						args.Add(true);
					} else if (Cmd.ToLower().StartsWith("false")) {
						Cmd = Cmd.Remove(0, 5);
						args.Add(false);
					} else {
						Render.DefaultSource.Instance.PushTextError("> Invalid argument");
						return;
					}

					if (Cmd.Length == 0)
						break;

					if (Cmd[0] != ',') {
						Render.DefaultSource.Instance.PushTextError("> Invalid argument seperator");
						return;
					}
					int emptyPad = 1;
					while (Cmd[emptyPad] == ' ' || Cmd[emptyPad] == '\t') emptyPad++;
					Cmd = Cmd.Remove(0, emptyPad);
				}
			}

			if (command == null) {
				Render.DefaultSource.Instance.PushTextError("> Command or alias \"" + cmdName + "\" not found");
				return;
			}

			List<TypeCode[]> targetFxs = new List<TypeCode[]>();
			foreach (var fx in command.IFace.ArgTypes) {
				if (fx.Length == args.Count) {
					bool valid = true;
					for (int x = 0;x < fx.Length;x++) {
						if (fx[x] != Type.GetTypeCode(args[x].GetType()))
							valid = false;
					}
					if (valid)
						targetFxs.Add(fx);
				}
			}

			if (targetFxs.Count == 0) {
				Render.DefaultSource.Instance.PushTextError("> No overloads of \"" + cmdName + "\" match input arguments");

				for (int x = 0;x < command.IFace.ArgTypes.Length;x++) {
					string genArgs = "";
					var fx = command.IFace.ArgTypes[x];
					genArgs += (x == 0 ? "" : ", ");
					foreach (var arg in fx) {
						switch (arg) {
							case TypeCode.Boolean:
								genArgs += "bool";
								break;
							case TypeCode.Char:
								genArgs += "char";
								break;
							case TypeCode.Decimal:
								genArgs += "decimal";
								break;
							case TypeCode.Int64:
								genArgs += "long";
								break;
							case TypeCode.String:
								genArgs += "string";
								break;
							case TypeCode.UInt64:
								genArgs += "ulong";
								break;
						}
					}
					Render.DefaultSource.Instance.PushTextError(">\t" + cmdName + "(" + genArgs + ")");
				}
				

				return;
			}

			CommandExecuting = true; {
				var cancel = new CancellationTokenSource();
				if (command.IFace is ILongExecutableCommand) {
					var task = ((ILongExecutableCommand)command.IFace).Execute(args.ToArray(), cancel.Token);
					var wait = new Render.WaitingBoxOverlay("Executing " + cmdName + "...", cancel);
					if (await Task.WhenAny(task, Task.Delay(500)) != task) {
						wait.Display();
						try {
							await task;
						} catch (Exception ex) {
							Render.DefaultSource.Instance.PushTextError("Command failed: \n" + ex);
						}
						wait.Dispose();
					}
				} else {
					try {
						await ((IExecutableCommand)command.IFace).Execute(args.ToArray());
					} catch (Exception ex) {
						Render.DefaultSource.Instance.PushTextError("Command failed: \n" + ex);
					}
				}
			} CommandExecuting = false;
		}
	}
}
