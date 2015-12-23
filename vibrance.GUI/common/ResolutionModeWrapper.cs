using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vibrance.GUI.common
{
    public class ResolutionModeWrapper
    {
        public uint dmPelsWidth { get; set; }
        public uint dmPelsHeight { get; set; }
        public uint dmBitsPerPel { get; set; }
        public uint dmDisplayFrequency { get; set; }
        public uint dmDisplayFixedOutput { get; set; }

        public ResolutionModeWrapper() { }

        public ResolutionModeWrapper(DEVMODE mode)
        {
            this.dmPelsWidth = mode.dmPelsWidth;
            this.dmPelsHeight = mode.dmPelsHeight;
            this.dmBitsPerPel = mode.dmBitsPerPel;
            this.dmDisplayFrequency = mode.dmDisplayFrequency;
            this.dmDisplayFixedOutput = mode.dmDisplayFixedOutput;
        }

        public override string ToString()
        {
            return String.Format("{0} x {1} @ {3} hz ({2} bit, {4})", this.dmPelsWidth, this.dmPelsHeight, 
                this.dmBitsPerPel, this.dmDisplayFrequency, Enum.GetName(typeof(DMDFO), this.dmDisplayFixedOutput));
        }

        public override bool Equals(object obj)
        {
            ResolutionModeWrapper that = null;

            //if the object is of type DEVMODE, it corresponding ResolutionModeWrapper 
            //will be determined and the second check will always pass
            if (obj is DEVMODE)
            {
                that = new ResolutionModeWrapper((DEVMODE)obj);
            }
            if (obj is ResolutionModeWrapper)
            {
                that = that == null ? obj as ResolutionModeWrapper : that;
                if (this.dmPelsWidth == that.dmPelsWidth &&
                    this.dmPelsHeight == that.dmPelsHeight &&
                    this.dmBitsPerPel == that.dmBitsPerPel &&
                    this.dmDisplayFrequency == that.dmDisplayFrequency &&
                    this.dmDisplayFixedOutput == that.dmDisplayFixedOutput)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
