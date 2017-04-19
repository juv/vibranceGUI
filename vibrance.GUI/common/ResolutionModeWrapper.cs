using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vibrance.GUI.common
{
    public class ResolutionModeWrapper
    {
        public uint DmPelsWidth { get; set; }
        public uint DmPelsHeight { get; set; }
        public uint DmBitsPerPel { get; set; }
        public uint DmDisplayFrequency { get; set; }
        public uint DmDisplayFixedOutput { get; set; }

        public ResolutionModeWrapper() { }

        public ResolutionModeWrapper(Devmode mode)
        {
            this.DmPelsWidth = mode.dmPelsWidth;
            this.DmPelsHeight = mode.dmPelsHeight;
            this.DmBitsPerPel = mode.dmBitsPerPel;
            this.DmDisplayFrequency = mode.dmDisplayFrequency;
            this.DmDisplayFixedOutput = mode.dmDisplayFixedOutput;
        }

        public override string ToString()
        {
            return String.Format("{0} x {1} @ {3} hz ({2} bit, {4})", this.DmPelsWidth, this.DmPelsHeight, 
                this.DmBitsPerPel, this.DmDisplayFrequency, Enum.GetName(typeof(Dmdfo), this.DmDisplayFixedOutput));
        }

        public override bool Equals(object obj)
        {
            ResolutionModeWrapper that = null;

            //if the object is of type DEVMODE, it corresponding ResolutionModeWrapper 
            //will be determined and the second check will always pass
            if (obj is Devmode)
            {
                that = new ResolutionModeWrapper((Devmode)obj);
            }
            if (obj is ResolutionModeWrapper || that != null)
            {
                that = that == null ? obj as ResolutionModeWrapper : that;
                if (this.DmPelsWidth == that.DmPelsWidth &&
                    this.DmPelsHeight == that.DmPelsHeight &&
                    this.DmBitsPerPel == that.DmBitsPerPel &&
                    this.DmDisplayFrequency == that.DmDisplayFrequency &&
                    this.DmDisplayFixedOutput == that.DmDisplayFixedOutput)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
