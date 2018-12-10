using System;
using System.Xml.Serialization;

namespace vibrance.GUI.common
{
    public class ApplicationSetting
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public int IngameLevel { get; set; }
        public bool IsResolutionChangeNeeded { get; set; }
        [XmlElement(IsNullable = true)]
        public ResolutionModeWrapper ResolutionSettings { get; set; }

        public ApplicationSetting(){ }

        public ApplicationSetting(string name, string fileName, int ingameLevel, ResolutionModeWrapper resolutionSettings, bool isResolutionChangeNeeded)
        {
            this.Name = name;
            this.FileName = fileName;
            this.IngameLevel = ingameLevel;
            this.ResolutionSettings = resolutionSettings;
            this.IsResolutionChangeNeeded = isResolutionChangeNeeded;
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            ApplicationSetting that = (ApplicationSetting)obj;
            return this.FileName.Equals(that.FileName);
        }

        public override int GetHashCode()
        {
            return this.FileName.GetHashCode();
        }
    }
}
