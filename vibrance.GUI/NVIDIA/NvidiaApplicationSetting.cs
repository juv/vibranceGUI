using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace vibrance.GUI.NVIDIA
{
    public class NvidiaApplicationSetting
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int IngameLevel { get; set; }

        public NvidiaApplicationSetting(){ }

        public NvidiaApplicationSetting(string name, string fileName, int ingameLevel)
        {
            this.Name = name;
            this.FileName = fileName;
            this.IngameLevel = ingameLevel;
        }
    }
}
