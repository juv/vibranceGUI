using System.Collections.Generic;

namespace vibrance.GUI
{
    class AmdSettingsWrapper
    {
        private int value;
        private string percentage = string.Empty;
        private const int AMD_DEFAULT_LEVEL = 50;

        private static List<AmdSettingsWrapper> settingsList;

        public AmdSettingsWrapper(int value, string percentage)
        {
            this.value = value;
            this.percentage = percentage;
        }

        public int getValue
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public string getPercentage
        {
            get { return this.percentage; }
            set { this.percentage = value; }
        }

        private static List<AmdSettingsWrapper> generateSettingsWrapper()
        {
            List<AmdSettingsWrapper> settingsWrapperList = new List<AmdSettingsWrapper>();
            List<int> staticValues = new List<int> {0,1,3,4,5,6,8,9,10,11,13,14,15,16,18,19,20,21,23,24,25,26,28,29,30,32,33,34,35,37,38,39,40,42,43,44,45,
                47,48,49,50,52,53,54,55,57,58,59,60,62,63};

            int percentageValue = AmdSettingsWrapper.AMD_DEFAULT_LEVEL;
            foreach (int value in staticValues)
            {
                settingsWrapperList.Add(new AmdSettingsWrapper(value, percentageValue + "%"));
                percentageValue++;
            }
            return settingsWrapperList;
        }

        public static AmdSettingsWrapper find(int value)
        {
            if (settingsList == null)
                settingsList = generateSettingsWrapper();
            AmdSettingsWrapper returnWrapper = settingsList.Find(x => x.getValue == value);

            // todo
            if (returnWrapper == null)
            {
                returnWrapper = find(value + 1);
            }
            return returnWrapper;
        }
    }
}
