using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Configuration
{
    public class SaveSettings : ScriptableObject
    {
        [Header("Platform")]
        public bool isForAllPlatforms;
        public RuntimePlatform runtimePlatform;

        [Header("Versioning")]
        public bool writebackToFileDisabled;

        [Header("Storage Settings")]
        public string fileExtensionName           = ".savegame";
        public string fileFolderName              = "Save Data";
        public string fileFolderNameEditor        = "Save Data - Editor";
        public string fileName                    = "Slot";
        public string specialDataFolderNameSuffix = "_Data";

        [Header("Configuration")]
        [Range(1, 300)]
        public int maxSaveSlotCount = 300;
        [Tooltip("The save system will increment the time played since load")]
        public bool trackTimePlayed = true;
        [Tooltip("When you disable this, writing the game only happens when you call SaveMaster.Save()")]
        public bool autoSaveOnExit = true;
        [Tooltip("Should the game get saved when switching between game saves?")]
        public bool autoSaveOnSlotSwitch = true;

        [Header("Auto Save")]
        [Tooltip("Automatically save to the active slot based on a time interval, useful for WEBGL games")]
        public bool saveOnInterval;
        [Tooltip("Time interval in seconds before the autosave happens"), Range(1, 3000)]
        public int saveIntervalTime = 1;

        [Header("Initialization")]
        public bool loadDefaultSlotOnStart = true;
        [Range(0, 299)]
        public int defaultSlot;

        [Header("Extras")]
        public bool useHotkeys;
        public KeyCode saveAndWriteToDiskKey = KeyCode.F2;
        public KeyCode syncSaveGameKey       = KeyCode.F4;
        public KeyCode syncLoadGameKey       = KeyCode.F5;

        [Header("Debug (Unity Editor Only)")]
        public bool showSaveFileUtilityLog;

        private void OnDestroy()
        {
            PlatformSpecificSaveSettingsProvider.ClearStaticVariables();
        }

        public static SaveSettings Get()
        {
            SaveSettings saveSettings = PlatformSpecificSaveSettingsProvider.GetSaveSettingOfCurrentPlatform();

#if UNITY_EDITOR

            // In case the settings are not found, we create one
            if (saveSettings == null)
            {
                return CreateFile();
            }
#endif

            // In case it still doesn't exist, somehow it got removed.
            // We send a default instance of SavePluginSettings.
            if (saveSettings == null)
            {
                Debug.LogWarning("Could not find 'SavePluginsSettings' in Resource folder. Using default settings.");
                saveSettings = CreateInstance<SaveSettings>();
            }

            return saveSettings;
        }

#if UNITY_EDITOR

        public static SaveSettings CreateFile()
        {
            string resourceFolderPath = $"{Application.dataPath}/{"Resources"}";
            string filePath           = $"{resourceFolderPath}/{"Save Settings.asset"}";

            if (!Directory.Exists(resourceFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!File.Exists(filePath))
            {
                SaveSettings instance = CreateInstance<SaveSettings>();
                instance.isForAllPlatforms = true;
                AssetDatabase.CreateAsset(instance, "Assets/Resources/Save Settings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return instance;
            }

            return Resources.Load("Save Settings", typeof(SaveSettings)) as SaveSettings;
        }

        private void OnValidate()
        {
            fileExtensionName = ValidateString(fileExtensionName, ".savegame", false);
            fileFolderName    = ValidateString(fileFolderName,    "SaveData",  true);
            fileName          = ValidateString(fileName,          "Slot",      true);

            if (fileExtensionName[0] != '.')
            {
                Debug.LogWarning("SaveSettings: File extension name needs to start with a .");
                fileExtensionName = $".{fileExtensionName}";
            }
        }

        private string ValidateString(string input, string defaultString, bool allowWhiteSpace)
        {
            if (string.IsNullOrEmpty(input) || !allowWhiteSpace && input.Any(Char.IsWhiteSpace))
            {
                Debug.LogWarning($"SaveSettings: Set {input} back to {defaultString} " + "since it was empty or has whitespace.");

                return defaultString;
            }

            return input;
        }

#endif
    }
}