using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data
{
    public class PlatformSpecificSaveSettingsProvider
    {
        private static Dictionary<RuntimePlatform, SaveSettings> _platformSaveSettings;
        private static SaveSettings                              _saveSettingsForAllPlatforms;

        public static void ClearStaticVariables()
        {
            _platformSaveSettings        = null;
            _saveSettingsForAllPlatforms = null;
        }

        public static SaveSettings GetSaveSettingOfCurrentPlatform()
        {
            InitializeIfNecessary();

            if (_platformSaveSettings.ContainsKey(Application.platform))
            {
                return _platformSaveSettings[Application.platform];
            }

            return _saveSettingsForAllPlatforms;
        }

        private static void InitializeIfNecessary()
        {
            if (_platformSaveSettings != null)
            {
                return;
            }

            _platformSaveSettings = new Dictionary<RuntimePlatform, SaveSettings>();

            SaveSettings[] saveSettings = Resources.LoadAll<SaveSettings>("");

            for (int i = 0; i < saveSettings.Length; i++)
            {
                if (saveSettings[i].isForAllPlatforms)
                {
                    if (_saveSettingsForAllPlatforms != null)
                    {
                        Debug.LogError("Found multiple save settings, which are flagged to be used for all platforms. Make sure you flag exactly one SaveSettings to be used for all platforms.");
                    }
                    else
                    {
                        _saveSettingsForAllPlatforms = saveSettings[i];
                    }
                }
                else
                {
                    _platformSaveSettings.Add(saveSettings[i].runtimePlatform, saveSettings[i]);
                }
            }

            if (_saveSettingsForAllPlatforms == null)
            {
                Debug.LogError("Found no SaveSettings flagged to be used for all platforms. Please make sure to flag exactly one SaveSettings to be used for all platforms.");
            }
        }
    }
}
