using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    class DeviceGammaRampHelper
    {

        public static void SetGammaRamp(Screen screen, int brightness = 50, int contrast = 50, int gamma = 100)
        {
            if(brightness == 0 || contrast == 0 || gamma == 0)
            {
                return;
            }
            SetGammaRamp(screen, (double)brightness / 100, (double)contrast / 100, (double)gamma / 100);
        }
        /// <summary>
        /// Applies gamma, brightness, contrast to the selected screen.
        /// </summary>
        /// <param name="screen">Screen to apply the gamma ramp to</param>
        /// <param name="gamma">Gamma, default value 1.</param>
        /// <param name="brightness">Brightness, default value 0.5</param>
        /// <param name="contrast">Contrast, default value 0.5.</param>
        public static void SetGammaRamp(Screen screen, double brightness = 0.5, double contrast = 0.5, double gamma = 1.0)
        {
            var hdc = GetScreenDeviceContext(screen);
            var newGammaRamp = CalculateGammaRamp(hdc, gamma, brightness, contrast);
            ApplyGammaRamp(hdc, newGammaRamp);
            ReleaseDeviceContext(hdc);
        }

        /// <summary>
        /// Gets the current gamma ramp for the selected screen.
        /// </summary>
        /// <param name="screen">Screen to get the gamma ramp of</param>
        public static GammaRamp GetGammaRamp(Screen screen)
        {
            GammaRamp currentGammaRamp = new GammaRamp(); 
            var hdc = GetScreenDeviceContext(screen);
            NativeAPI.GetDeviceGammaRamp(hdc, ref currentGammaRamp);
            ReleaseDeviceContext(hdc);
            return currentGammaRamp;
        }


        private static IntPtr GetScreenDeviceContext(Screen screen)
        {
            var hdc = NativeAPI.CreateDC(screen.DeviceName, null, null, IntPtr.Zero);
            if (hdc == IntPtr.Zero)
            {
                VibranceGUI.Log(string.Format("Failed to create screen device context for screen {0}", screen.DeviceName));
            }
            return hdc;
        }

        private static void ReleaseDeviceContext(IntPtr hdc)
        {
            if (NativeAPI.ReleaseDC(IntPtr.Zero, hdc) == 0)
            {
                VibranceGUI.Log(string.Format("Failed to release device context handle {0}", hdc.ToString()));
            }                
        }

        /// <summary>
        /// Calculate the device lookup table (LUT). Credits to https://github.com/falahati in https://github.com/falahati/NvAPIWrapper/issues/20#issuecomment-634551206
        /// </summary>
        /// <param name="brightness">The brightness value</param>
        /// <param name="contrast">The contrast value</param>
        /// <param name="gamma">The Gamma value</param>
        /// <returns></returns>
        private static ushort[] CalculateLUT(double brightness = 0.5, double contrast = 0.5, double gamma = 1)
        {

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
            offset += brightness * (range / 5);

            // Fill the gamma curve
            var result = new ushort[dataPoints];
            for (var i = 0; i < result.Length; i++)
            {
                var factor = (i + offset) / range;

                factor = Math.Pow(factor, 1 / gamma);

                factor = Math.Min(Math.Max(factor, 0), 1);

                result[i] = (ushort)Math.Round(factor * ushort.MaxValue);
            }

            return result;
        }

        private static GammaRamp CalculateGammaRamp(IntPtr hdc, double gamma, double brightness, double contrast)
        {
            ushort[] Red = new ushort[256];
            ushort[] Green = new ushort[256];
            ushort[] Blue = new ushort[256];

            Red = CalculateLUT(brightness, contrast, gamma);
            Green = CalculateLUT(brightness, contrast, gamma);
            Blue = CalculateLUT(brightness, contrast, gamma);

            GammaRamp ramp = new GammaRamp(Red, Green, Blue);
            return ramp;
        }

        private static void ApplyGammaRamp(IntPtr hdc, GammaRamp ramp)
        {
            if (!NativeAPI.SetDeviceGammaRamp(hdc, ref ramp))
            {
                VibranceGUI.Log(string.Format("Failed to set device gamma ramp for handle {0}", hdc.ToString()));
            }
        }

        public static bool IsGammaRampEqualToWindowsValues(VibranceInfo vibranceInfo, ApplicationSetting applicationSetting)
        {
            return vibranceInfo.userColorSettings.brightness == applicationSetting.Brightness && vibranceInfo.userColorSettings.contrast == applicationSetting.Contrast && vibranceInfo.userColorSettings.gamma == applicationSetting.Gamma;
        }

        public static bool IsGammaRampDefault(VibranceInfo vibranceInfo)
        {
            return 50 == vibranceInfo.userColorSettings.brightness && 50 == vibranceInfo.userColorSettings.contrast && 100 == vibranceInfo.userColorSettings.gamma;
        }

        // constant data
        public const int GAMMA_RAMP_SIZE = 256;

        // types
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct GammaRamp
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GAMMA_RAMP_SIZE)]
            public UInt16[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GAMMA_RAMP_SIZE)]
            public UInt16[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GAMMA_RAMP_SIZE)]
            public UInt16[] Blue;

            // constructor
            /// <summary>
            /// Define red, blue and green arrays.
            /// </summary>
            /// <param name="r">Red array.</param>
            /// <param name="g">Green array.</param>
            /// <param name="b">Blue array.</param>
            public GammaRamp(UInt16[] r = null, UInt16[] g = null, UInt16[] b = null)
            {
                Red = r == null ? new UInt16[GAMMA_RAMP_SIZE] : r;
                Green = g == null ? new UInt16[GAMMA_RAMP_SIZE] : g;
                Blue = b == null ? new UInt16[GAMMA_RAMP_SIZE] : b;
            }

            public override bool Equals(object obj)
            {
                if (obj is GammaRamp)
                {
                    return false;
                }
                GammaRamp other = (GammaRamp)obj;
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


        // Windows Native API
        private class NativeAPI
        {
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

            [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
            public static extern int ReleaseDC([In] IntPtr hWnd, [In] IntPtr hDC);

            [DllImport("user32.dll", EntryPoint = "GetDC")]
            public static extern IntPtr GetDC([In] IntPtr hWnd);


            // extern methods
            [DllImport("gdi32.dll")]
            public static extern bool SetDeviceGammaRamp(IntPtr hDC, ref GammaRamp lpRamp);
            [DllImport("gdi32.dll")]
            public static extern bool GetDeviceGammaRamp(IntPtr hDC, ref GammaRamp lpRamp);
        }
    }
}
