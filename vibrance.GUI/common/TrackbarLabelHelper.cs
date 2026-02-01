using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{
    internal class TrackbarLabelHelper
    {
        public static string ResolveVibranceLabelLevel(GraphicsAdapter graphicsAdapter, int value)
        {
            switch (graphicsAdapter)
            {
                case GraphicsAdapter.Nvidia:
                    return NvidiaVibranceValueWrapper.Find(value).Percentage;
                case GraphicsAdapter.Amd:
                    return ResolvePercentageLabelLevel(value);
                case GraphicsAdapter.Ambiguous:
                case GraphicsAdapter.Unknown:
                default:
                    return "";
            }
        }

        public static string ResolveBrightnessLabelLevel(int value)
        {
            return ResolvePercentageLabelLevel(value);
        }

        public static string ResolveContrastLabelLevel(int value)
        {
            return ResolvePercentageLabelLevel(value);
        }

        public static string ResolveGammaLabelLevel(int value)
        {
            return string.Format("{0:F2}", (double)value / 100);
        }

        private static string ResolvePercentageLabelLevel(int value)
        {
            return string.Format("{0}%", value);
        }
    }
}
