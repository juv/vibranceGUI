using System.Collections.Generic;

namespace vibrance.GUI.NVIDIA
{
    class NvidiaVibranceValueWrapper
    {
        private const int NvapiDefaultLevel = 50;

        private static List<NvidiaVibranceValueWrapper> _settingsList;

        public NvidiaVibranceValueWrapper(int value, string percentage)
        {
            this.Value = value;
            this.Percentage = percentage;
        }

        public int Value { get; set; }

        public string Percentage { get; set; } = string.Empty;

        private static List<NvidiaVibranceValueWrapper> GenerateSettingsWrapper()
        {
            List<NvidiaVibranceValueWrapper> settingsWrapperList = new List<NvidiaVibranceValueWrapper>();
            List<int> staticValues = new List<int> {0,1,3,4,5,6,8,9,10,11,13,14,15,16,18,19,20,21,23,24,25,26,28,29,30,32,33,34,35,37,38,39,40,42,43,44,45,
                47,48,49,50,52,53,54,55,57,58,59,60,62,63};

            int percentageValue = NvapiDefaultLevel;
            foreach (int value in staticValues)
            {
                settingsWrapperList.Add(new NvidiaVibranceValueWrapper(value, percentageValue + "%"));
                percentageValue++;
            }
            return settingsWrapperList;
        }

        public static NvidiaVibranceValueWrapper Find(int value)
        {
            if (_settingsList == null)
                _settingsList = GenerateSettingsWrapper();
            NvidiaVibranceValueWrapper returnWrapper = _settingsList.Find(x => x.Value == value) ?? Find(value + 1);

            return returnWrapper;
        }
    }
}
