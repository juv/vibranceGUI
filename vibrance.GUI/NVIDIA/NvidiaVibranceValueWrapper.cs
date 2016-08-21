using System.Collections.Generic;

namespace vibrance.GUI.NVIDIA
{
    class NvidiaVibranceValueWrapper
    {
        private int _value;
        private string _percentage = string.Empty;
        private const int NvapiDefaultLevel = 50;

        private static List<NvidiaVibranceValueWrapper> _settingsList;

        public NvidiaVibranceValueWrapper(int value, string percentage)
        {
            this._value = value;
            this._percentage = percentage;
        }

        public int GetValue
        {
            get { return this._value; }
            set { this._value = value; }
        }

        public string GetPercentage
        {
            get { return this._percentage; }
            set { this._percentage = value; }
        }

        private static List<NvidiaVibranceValueWrapper> GenerateSettingsWrapper()
        {
            List<NvidiaVibranceValueWrapper> settingsWrapperList = new List<NvidiaVibranceValueWrapper>();
            List<int> staticValues = new List<int> {0,1,3,4,5,6,8,9,10,11,13,14,15,16,18,19,20,21,23,24,25,26,28,29,30,32,33,34,35,37,38,39,40,42,43,44,45,
                47,48,49,50,52,53,54,55,57,58,59,60,62,63};

            int percentageValue = NvidiaVibranceValueWrapper.NvapiDefaultLevel;
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
            NvidiaVibranceValueWrapper returnWrapper = _settingsList.Find(x => x.GetValue == value);

            //some values are not stored by nvidia internally. use the value below that. 
            if (returnWrapper == null)
            {
                returnWrapper = Find(value + 1);
            }
            return returnWrapper;
        }
    }
}
