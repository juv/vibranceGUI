using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using vibrance.GUI.common;

namespace vibrance.GUI.NVIDIA
{
    public class NvidiaApplicationSetting
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int IngameLevel { get; set; }
        public bool IsResolutionChangeNeeded { get; set; }
        [XmlElement(IsNullable = true)]
        public ResolutionModeWrapper ResolutionSettings { get; set; }

        public NvidiaApplicationSetting(){ }

        public NvidiaApplicationSetting(string name, string fileName, int ingameLevel, ResolutionModeWrapper resolutionSettings, bool isResolutionChangeNeeded)
        {
            this.Name = name;
            this.FileName = fileName;
            this.IngameLevel = ingameLevel;
            this.ResolutionSettings = resolutionSettings;
            this.IsResolutionChangeNeeded = isResolutionChangeNeeded;
        }
    }
}
