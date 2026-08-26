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

        // Used by ResolutionHelper.ChangeResolutionEx/IsResolutionChangeNeeded to decide whether a
        // mode change is still needed and whether one that was just attempted actually landed -
        // deliberately comparing only the four fields a change actually declares and can verify
        // (DmPelsWidth, DmPelsHeight, DmBitsPerPel, DmDisplayFrequency), NOT DmDisplayFixedOutput,
        // unlike Equals above. DmDisplayFixedOutput (the "(Center)"/"(Stretch)" scaling choice) is
        // only honoured by ChangeDisplaySettingsEx when DM_DISPLAYFIXEDOUTPUT survives into the
        // achieved mode's own dmFields, which is driver-dependent - some drivers apply the four
        // real fields correctly but silently pin this one to their own default regardless of what
        // was requested. Basing the "does this still need changing?" guard on a field a driver is
        // free to never honour is what let a user's "(Center)" mode selection re-fire a real mode
        // set and registry write on every single foreground event, forever, even though the mode
        // had genuinely already been achieved on every field the driver actually supports.
        // Equals/ToString above are intentionally untouched by this - the combo box in
        // VibranceSettings and the applicationData.xml round trip both depend on all five fields
        // matching exactly.
        public bool MatchesAchievedMode(Devmode mode)
        {
            return this.DmPelsWidth == mode.dmPelsWidth &&
                this.DmPelsHeight == mode.dmPelsHeight &&
                this.DmBitsPerPel == mode.dmBitsPerPel &&
                this.DmDisplayFrequency == mode.dmDisplayFrequency;
        }
    }
}
