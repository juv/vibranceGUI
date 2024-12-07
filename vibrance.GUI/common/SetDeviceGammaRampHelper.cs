using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    class SetDeviceGammaRampHelper
    {

		static int monitorId;
		static double level;
		static double gamma;
		static double brightness;
		static double contrast;
		static int monitorCounter;

		/// <summary>
		/// Applies level, gamma, brightness, contrast to selected monitor.
		/// </summary>
		/// <param name="monitorId">Id of selected monitor. If id is 0 then settings are applied to all monitors.</param>
		/// <param name="level">Level, default value 50</param>
		/// <param name="gamma">Gamma, default value 10.</param>
		/// <param name="brightness">Brightness, default value 50</param>
		/// <param name="contrast">Contrast, default value 50.</param>
		public static void ApplyGammaRamp(int monitorId, double level, double gamma, double brightness, double contrast)
		{
			SetDeviceGammaRampHelper.monitorId = monitorId;
			SetDeviceGammaRampHelper.level = level;
			SetDeviceGammaRampHelper.gamma = gamma;
			SetDeviceGammaRampHelper.brightness = brightness;
			SetDeviceGammaRampHelper.contrast = contrast;
			monitorCounter = 0;

			var hdc = NativeAPI.GetDC(IntPtr.Zero);
			if (hdc == IntPtr.Zero)
				throw new InvalidOperationException();
			if (!NativeAPI.EnumDisplayMonitors(hdc, IntPtr.Zero, MonitorEnumProc, IntPtr.Zero))
				throw new InvalidOperationException();
			if (NativeAPI.ReleaseDC(IntPtr.Zero, hdc) == 0)
				throw new InvalidOperationException();
		}

		static NativeAPI.RAMP withoutRamp = new NativeAPI.RAMP();

		// MonitorEnumProc callback
		private static int MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref NativeAPI.tagRECT lprcMonitor, IntPtr dwData)
		{
			//var info = new NativeAPI.MonitorInfo { cbSize = (uint)Marshal.SizeOf(typeof(NativeAPI.MonitorInfo)) };
			//if (!NativeAPI.GetMonitorInfoW(param0, ref info))
			//	throw new InvalidOperationException();

			
			IntPtr thisHandle = NativeAPI.GetForegroundWindow();
			Screen screen = Screen.FromHandle(thisHandle);
			screen.ToString();

			monitorCounter++;

			//if (monitorId == 0 || monitorCounter == monitorId)
			//{
				

			if(withoutRamp.Red == null)
            {
				bool ret = NativeAPI.GetDeviceGammaRamp(hdcMonitor, ref withoutRamp);
				string content = "";
				if (ret)
				{
					content += string.Join(",\n", withoutRamp.Red);
					content += string.Join(",\n", withoutRamp.Green);
					content += string.Join(",\n", withoutRamp.Blue);
					File.WriteAllText("data-ohne" + monitorCounter + ".txt", content);
				}
			}
            else
            {
				NativeAPI.RAMP ramp = new NativeAPI.RAMP();
				bool ret = NativeAPI.GetDeviceGammaRamp(hdcMonitor, ref ramp);
				string content = "";
				if (ret)
				{
					content += string.Join(",\n", ramp.Red);
					content += string.Join(",\n", ramp.Green);
					content += string.Join(",\n", ramp.Blue);
					File.WriteAllText("data" + monitorCounter + "-" + DateTime.Now.ToFileTime().ToString() + ".txt", content);
				}

				if(withoutRamp.Equals(ramp))
                {
					Console.WriteLine("equal");
                }
			}

			CalculateRamp(hdcMonitor, level, gamma, brightness, contrast);
			//}

			return 1;
		}

		private static ushort[] CalculateLUT(double brightness = 0.5, double contrast = 0.5, double gamma = 1) { 

		const int dataPoints = 256;

			// Limit gamma in range [0.4-2.8]
			gamma = Math.Min(Math.Max(gamma, 0.4), 2.8);

		// Normalize contrast in range [-1,1]
		contrast = (Math.Min(Math.Max(contrast, 0), 1) - 0.5) * 2;

		// Normalize brightness in range [-1,1]
		brightness = (Math.Min(Math.Max(brightness, 0), 1) - 0.5) * 2;

		// Calculate curve offset resulted from contrast
		var offset = contrast > 0 ? contrast * -25.4 : contrast * -32;

			// Calculate the total range of curve
			var range = (dataPoints - 1) + offset * 2;

			// Add brightness to the curve offset
			offset += brightness* (range / 5);

		// Fill the gamma curve
		var result = new ushort[dataPoints];
		for (var i = 0; i<result.Length; i++)
		{
			var factor = (i + offset) / range;

			factor = Math.Pow(factor, 1 / gamma);

			factor = Math.Min(Math.Max(factor, 0), 1);

			result[i] = (ushort) Math.Round(factor* ushort.MaxValue);
		}

		return result;
	}

private static void CalculateRamp(IntPtr hdc, double level, double gamma, double brightness, double contrast)
		{
			ushort[] Red = new ushort[256];
			ushort[] Green = new ushort[256];
			ushort[] Blue = new ushort[256];

			/*gamma /= 10;
			brightness = 1 + (((brightness - 50) / 100) * 65535);
			contrast = 1 + ((contrast - 50) / 100);
			level = 1 + ((level - 50) / 100);

			for (int i = 0; i < 256; i++)
			{
				double value = i * 256;
				value = (Math.Pow(value / 65535, 1 / gamma) * 65535) + 0.5;
				value = ((((value / 65535) - 0.5) * contrast) + 0.5) * 65535;
				value = value += brightness;
				value *= level;
				Red[i] = Green[i] = Blue[i] = (ushort)Math.Min((double)65535, Math.Max((double)0, value));
			}*/

			Red = CalculateLUT(brightness, contrast, gamma);
		    Green = CalculateLUT(brightness, contrast, gamma);
			Blue = CalculateLUT(brightness, contrast, gamma);

			NativeAPI.RAMP ramp = new NativeAPI.RAMP(Red, Green, Blue);
			if (!NativeAPI.SetDeviceGammaRamp(hdc, ref ramp))
				throw new InvalidOperationException();
		}


		// Windows Native API
		private class NativeAPI
		{
			[DllImport("user32.dll")]
			public static extern IntPtr GetForegroundWindow();

			[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
			public static extern int ReleaseDC([In] IntPtr hWnd, [In] IntPtr hDC);

			[DllImport("user32.dll", EntryPoint = "GetDC")]
			public static extern IntPtr GetDC([In] IntPtr hWnd);

			[DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetMonitorInfoW([In] IntPtr hMonitor, ref MonitorInfo lpmi);

			[DllImport("user32.dll", EntryPoint = "EnumDisplayMonitors")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool EnumDisplayMonitors([In] IntPtr hdc, [In] IntPtr lprcClip, MONITORENUMPROC lpfnEnum,
				IntPtr dwData);



			// types
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
			public struct RAMP
			{
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = RAMP_SZ)]
				public UInt16[] Red;
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = RAMP_SZ)]
				public UInt16[] Green;
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = RAMP_SZ)]
				public UInt16[] Blue;

				// constructor
				/// <summary>
				/// Define red, blue and green arrays.
				/// </summary>
				/// <param name="r">Red array.</param>
				/// <param name="g">Green array.</param>
				/// <param name="b">Blue array.</param>
				public RAMP(UInt16[] r = null, UInt16[] g = null, UInt16[] b = null)
				{
					Red = r == null ? new UInt16[RAMP_SZ] : r;
					Green = g == null ? new UInt16[RAMP_SZ] : g;
					Blue = b == null ? new UInt16[RAMP_SZ] : b;
				}

                public override bool Equals(object obj)
                {
					if(obj is RAMP)
                    {
						return false;
                    }
					RAMP other = (RAMP)obj;
                    return this.Red.Equals(other.Red) && this.Blue.Equals(other.Blue) && this.Green.Equals(other.Green);
                }

                public override int GetHashCode()
                {
                    int hashCode = -1058441243;
                    hashCode = hashCode * -1521134295 + EqualityComparer<ushort[]>.Default.GetHashCode(Red);
                    hashCode = hashCode * -1521134295 + EqualityComparer<ushort[]>.Default.GetHashCode(Green);
                    hashCode = hashCode * -1521134295 + EqualityComparer<ushort[]>.Default.GetHashCode(Blue);
                    return hashCode;
                }
            };


			// constant data
			public const int RAMP_SZ = 256;


			// extern methods
			[DllImport("gdi32.dll")]
			public static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);
			[DllImport("gdi32.dll")]
			public static extern bool GetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

			[UnmanagedFunctionPointer(CallingConvention.StdCall)]
			public delegate int MONITORENUMPROC(IntPtr param0, IntPtr param1, ref tagRECT param2, IntPtr param3);

			[StructLayout(LayoutKind.Sequential)]
			public struct MonitorInfo
			{
				public uint cbSize;
				public tagRECT rcMonitor;
				public tagRECT rcWork;
				public uint dwFlags;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct tagRECT
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}
		}

	}
}
