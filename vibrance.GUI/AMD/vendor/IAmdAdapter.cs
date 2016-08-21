using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vibrance.GUI.AMD.vendor
{
    public interface IAmdAdapter : IDisposable
    {
        void SetSaturationOnAllDisplays(int vibranceLevel);

        void SetSaturationOnDisplay(int vibranceLevel, string displayName);

        bool IsAvailable();

        void Init();
    }
}