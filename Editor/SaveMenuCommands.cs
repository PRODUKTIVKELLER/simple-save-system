using Produktivkeller.SimpleSaveSystem.Configuration;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Editor
{
    public class SaveMenuCommands
    {
        [MenuItem(itemName: "PRODUKTIVKELLER/Simple Save System/Open Save Location")]
        public static void OpenSaveLocation()
        {
            string dataPath = $"{Application.persistentDataPath}/{SaveSettings.Get().fileFolderName}/";

#if UNITY_EDITOR_WIN
            dataPath = dataPath.Replace(@"/", @"\"); // Windows uses backward slashes
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            dataPath = dataPath.Replace("\\", "/"); // Linux and MacOS use forward slashes
#endif

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            EditorUtility.RevealInFinder(dataPath);
        }

        [MenuItem(itemName: "PRODUKTIVKELLER/Simple Save System/Open Save Settings")]
        public static void OpenSaveSystemSettings()
        {
            Selection.activeInstanceID = SaveSettings.Get().GetInstanceID();
        }
    }
}