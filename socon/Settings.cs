using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonConfig;
using System.IO;

namespace socon
{
	static class Settings
	{
		public static dynamic Global;
		public static ConfigObject GlobalNonDynamic;

		public static class Text
		{
			public static string FontFamily;
			public static float FontSize;
		}

		public static class Visual
		{
			public static bool VSync;
		}

		public static class Colors
		{
			public static ConsoleColor Background;
			public static ConsoleColor NormalText;
			public static ConsoleColor ErrorText;
			public static ConsoleColor CenterText;
			public static ConsoleColor PidBox;

			public static class CPProcList
			{
				public static ConsoleColor ProcessUntrusted;
				public static ConsoleColor ProcessLow;
				public static ConsoleColor ProcessMedium;
				public static ConsoleColor ProcessMediumPlus;
				public static ConsoleColor ProcessHigh;
				public static ConsoleColor ProcessSystem;
				public static ConsoleColor ProcessProtected;
				public static ConsoleColor ProcessUnknown;
			}

			public static class ConfirmBox
			{
				public static ConsoleColor Background;
				public static ConsoleColor Border;
				public static ConsoleColor YesButtonActiveBackground;
				public static ConsoleColor YesButtonBackground;
				public static ConsoleColor YesButtonBorder;
				public static ConsoleColor NoButtonActiveBackground;
				public static ConsoleColor NoButtonBackground;
				public static ConsoleColor NoButtonBorder;
				public static ConsoleColor YesButtonText;
				public static ConsoleColor NoButtonText;
				public static ConsoleColor Text;
			}
		}

		public static class Keyboard
		{
			public static ulong PressedInHoldTime;
			public static ulong PressedInInterval;
		}

		public static void LoadSettings()
		{
			//try {
				Global = Config.ApplyJson(File.ReadAllText("settings.conf"), new ConfigObject());
				GlobalNonDynamic = Global;
			/*} catch {
				return;
			}*/

			Text.FontFamily = Global.Text.FontFamily ?? "";
			Text.FontSize = (float)(Global.Text.FontSize as double? ?? 12.0);

			Colors.Background = Base.Brushes.GetColorByString(Global.Text.Colors.Background as string ?? "Black");
			Colors.NormalText = Base.Brushes.GetColorByString(Global.Text.Colors.NormalText as string ?? "DarkGreen");
			Colors.ErrorText = Base.Brushes.GetColorByString(Global.Text.Colors.ErrorText as string ?? "DarkRed");
			Colors.CenterText = Base.Brushes.GetColorByString(Global.Text.Colors.CenterText as string ?? "DarkYellow");
			Colors.PidBox = Base.Brushes.GetColorByString(Global.Text.Colors.PidBox as string ?? "DarkRed");

			Colors.CPProcList.ProcessUntrusted = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.Untrusted as string ?? "DarkGray");
			Colors.CPProcList.ProcessLow = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.Low as string ?? "Gray");
			Colors.CPProcList.ProcessMedium = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.Medium as string ?? "White");
			Colors.CPProcList.ProcessMediumPlus = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.MediumPlus as string ?? "Yellow");
			Colors.CPProcList.ProcessHigh = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.High as string ?? "Blue");
			Colors.CPProcList.ProcessSystem = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.System as string ?? "DarkGreen");
			Colors.CPProcList.ProcessProtected = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.Protected as string ?? "Magenta");
			Colors.CPProcList.ProcessUnknown = Base.Brushes.GetColorByString(Global.Text.Colors.CPProcList.Unknown as string ?? "Red");

			Colors.ConfirmBox.Background = ConsoleColor.DarkRed;
			Colors.ConfirmBox.Border = ConsoleColor.Red;
			Colors.ConfirmBox.YesButtonActiveBackground = ConsoleColor.Magenta;
			Colors.ConfirmBox.YesButtonBackground = ConsoleColor.DarkRed;
			Colors.ConfirmBox.YesButtonBorder = ConsoleColor.Red;
			Colors.ConfirmBox.NoButtonActiveBackground = ConsoleColor.Magenta;
			Colors.ConfirmBox.NoButtonBackground = ConsoleColor.DarkRed;
			Colors.ConfirmBox.NoButtonBorder = ConsoleColor.Red;
			Colors.ConfirmBox.YesButtonText = Colors.NormalText;
			Colors.ConfirmBox.NoButtonText = Colors.NormalText;
			Colors.ConfirmBox.Text = Colors.NormalText;

			Visual.VSync = Global.Visual.VSync as bool? ?? true;

			Keyboard.PressedInHoldTime = Global.Keyboard.PressedInHoldTime as ulong? ?? 300;
			Keyboard.PressedInInterval = Global.Keyboard.PressedInInterval as ulong? ?? 50;
		}
	}
}
