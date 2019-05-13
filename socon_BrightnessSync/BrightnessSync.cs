using socon.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socon_BrightnessSync
{
	public class BrightnessSync : ISoconPlugin
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PHYSICAL_MONITOR {
			public IntPtr hPhysicalMonitor;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] 
			public string szPhysicalMonitorDescription;
		}
		[DllImport("Dxva2.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

		[DllImport("Dxva2.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

		[StructLayout(LayoutKind.Sequential)]
		public struct Rect {
			public int left;
			public int top;
			public int right;
			public int bottom;
		}
		public delegate bool EnumDisplayMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumDisplayMonitorsDelegate lpfnEnum, IntPtr dwData);

		[DllImport("Dxva2.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool GetMonitorBrightness(IntPtr hPhysicalMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

		[DllImport("Dxva2.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);

		[Flags]
		public enum MONITOR_CAPABILITIES : uint {
			MC_CAPS_NONE = 0x00000000,
			MC_CAPS_MONITOR_TECHNOLOGY_TYPE = 0x00000001,
			MC_CAPS_BRIGHTNESS = 0x00000002,
			MC_CAPS_CONTRAST = 0x00000004,
			MC_CAPS_COLOR_TEMPERATURE = 0x00000008,
			MC_CAPS_RED_GREEN_BLUE_GAIN = 0x00000010,
			MC_CAPS_RED_GREEN_BLUE_DRIVE = 0x00000020,
			MC_CAPS_DEGAUSS = 0x00000040,
			MC_CAPS_DISPLAY_AREA_POSITION = 0x00000080,
			MC_CAPS_DISPLAY_AREA_SIZE = 0x00000100,
			MC_CAPS_RESTORE_FACTORY_DEFAULTS = 0x00000400,
			MC_CAPS_RESTORE_FACTORY_COLOR_DEFAULTS = 0x00000800,
			MC_RESTORE_FACTORY_DEFAULTS_ENABLES_MONITOR_SETTINGS = 0x00001000     
		}

		[Flags]
		public enum MONITOR_SUPPORTED_COLOR_TEMPERATURE : uint {
			MC_SUPPORTED_COLOR_TEMPERATURE_NONE = 0x00000000,
			MC_SUPPORTED_COLOR_TEMPERATURE_4000K = 0x00000001,
			MC_SUPPORTED_COLOR_TEMPERATURE_5000K = 0x00000002,
			MC_SUPPORTED_COLOR_TEMPERATURE_6500K = 0x00000004,
			MC_SUPPORTED_COLOR_TEMPERATURE_7500K = 0x00000008,
			MC_SUPPORTED_COLOR_TEMPERATURE_8200K = 0x00000010,
			MC_SUPPORTED_COLOR_TEMPERATURE_9300K = 0x00000020,
			MC_SUPPORTED_COLOR_TEMPERATURE_10000K = 0x00000040,
			MC_SUPPORTED_COLOR_TEMPERATURE_11500K = 0x00000080
		}

		[DllImport("Dxva2.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool GetMonitorCapabilities(IntPtr hPhysicalMonitor, out MONITOR_CAPABILITIES pdwMonitorCapabilities, out MONITOR_SUPPORTED_COLOR_TEMPERATURE pdwSupportedColorTemperatures);

		[DllImport("Dxva2.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SetMonitorBrightness(IntPtr hPhysicalMonitor, uint dwNewBrightness);

		public string Name => "Multi-monitor brightness synchronization";

		private ManagementEventWatcher WMIWatcher;

		public void Init()
		{
			new Thread(() => {
				var scope = new ManagementScope("\\\\localhost\\root\\WMI", null);
				scope.Connect();
					
				WMIWatcher = new ManagementEventWatcher(scope, new EventQuery("SELECT * FROM WmiMonitorBrightnessEvent"));
				WMIWatcher.EventArrived += new EventArrivedEventHandler(EvBrightnessChange);
				WMIWatcher.Start();
			}).Start();
		}

		public void Unload() => WMIWatcher.Stop();

		private void EvBrightnessChange(object sender, EventArrivedEventArgs e)
        {
			var arrhMonitor = new List<IntPtr>();
			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) => {
				arrhMonitor.Add(hMonitor);
				return true;
			}, IntPtr.Zero);

			var arrPhysicalMonitor = new List<PHYSICAL_MONITOR>();
			foreach (var hMonitor in arrhMonitor) {
				uint sz;
				if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out sz)) {
					var lastError = Marshal.GetLastWin32Error();
					Text.PushTextError("BrightnessSync::GetNumberOfPhysicalMonitorsFromHMONITOR failed (0x" + lastError.ToString("X8") + ": " + new Win32Exception(lastError).Message + ")");
					return;
				}

				PHYSICAL_MONITOR[] pPhysicalMonitorArray = new PHYSICAL_MONITOR[sz];

				if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, sz, pPhysicalMonitorArray)) {
					var lastError = Marshal.GetLastWin32Error();
					Text.PushTextError("BrightnessSync::GetPhysicalMonitorsFromHMONITOR failed (0x" + lastError.ToString("X8") + ": " + new Win32Exception(lastError).Message + ")");
					return;
				}

				arrPhysicalMonitor.AddRange(pPhysicalMonitorArray);
			}

			var brightnessPercent = (byte)e.NewEvent.Properties["Brightness"].Value;

			foreach (var physicalMonitor in arrPhysicalMonitor) {
				/*MONITOR_CAPABILITIES caps;
				MONITOR_SUPPORTED_COLOR_TEMPERATURE temp;
				if (!GetMonitorCapabilities(physicalMonitor.hPhysicalMonitor, out caps, out temp)) {
					var lastError = Marshal.GetLastWin32Error();
					Text.PushTextError("BrightnessSync::GetMonitorCapabilities failed (0x" + lastError.ToString("X8") + ": " + new Win32Exception(lastError).Message + ")");
					continue;
				}

				if (!caps.HasFlag(MONITOR_CAPABILITIES.MC_CAPS_BRIGHTNESS)) {
					Text.PushText("Monitor " + physicalMonitor.szPhysicalMonitorDescription + " (0x" + physicalMonitor.hPhysicalMonitor.ToString("X8") + ") doesn't support brightness set/get", ConsoleColor.DarkYellow);
					continue;
				}*/

				uint minBrightness, curBrightness, maxBrightness;
				if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out minBrightness, out curBrightness, out maxBrightness)) {
					var lastError = Marshal.GetLastWin32Error();
					Text.PushTextError("BrightnessSync::GetMonitorBrightness failed (0x" + lastError.ToString("X8") + ": " + new Win32Exception(lastError).Message + ")");
					continue;
				}

				if (!SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, (uint)(brightnessPercent * ((float)maxBrightness / 100)))) {
					var lastError = Marshal.GetLastWin32Error();
					Text.PushTextError("BrightnessSync::SetMonitorBrightness failed (0x" + lastError.ToString("X8") + ": " + new Win32Exception(lastError).Message + ")");
					continue;
				}

				Text.ImportantMessageAddTimeout(physicalMonitor.szPhysicalMonitorDescription + " -> " + (uint)(brightnessPercent * ((float)maxBrightness / 100)), TimeSpan.FromMilliseconds(500));
				//Text.PushTextNormal(physicalMonitor.szPhysicalMonitorDescription + " -> min: " + minBrightness + ", cur: " + curBrightness + ", max: " + maxBrightness);
			}

			if (!DestroyPhysicalMonitors((uint)arrPhysicalMonitor.Count, arrPhysicalMonitor.ToArray())) {
				var lastError = Marshal.GetLastWin32Error();
				Text.PushTextError("BrightnessSync::DestroyPhysicalMonitors failed (0x" + lastError.ToString("X8") + ": " + new Win32Exception(lastError).Message + ")");
			}
        }
	}
}
