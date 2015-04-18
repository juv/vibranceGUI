using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vibrance.GUI.AMD.vendor
{
    public abstract class AmdAdapter
    {
        public abstract void SetSaturationOnAllDisplays(int vibranceLevel);

        public abstract void SetSaturationOnDisplay(int vibranceLevel, string displayName);
    }
}
