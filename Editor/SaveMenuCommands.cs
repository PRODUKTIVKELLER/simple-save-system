using Produktivkeller.SimpleSaveSystem.Configuration;
using Produktivkeller.SimpleSaveSystem.Core;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Editor
{
    public class SaveMenuCommands
    {
        [MenuItem(itemName: "PRODUKTIVKELLER/Simple Save System/Open Save Location")]
        public static void OpenSaveLocation()
        {
            string dataPath = string.Format("{0}/{1}/", Application.persistentDataPath, SaveSettings.Get().fileFolderName);

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

        [MenuItem(itemName: "PRODUKTIVKELLER/Simple Save System/Utility/Wipe Save Identifications (Active Scene)")]
        public static void WipeSceneSaveIdentifications()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            int rootObjectCount = rootObjects.Length;

            // Get all Saveables, including children and inactive.
            for (int i = 0; i < rootObjectCount; i++)
            {
                foreach (Saveable item in rootObjects[i].GetComponentsInChildren<Saveable>(true))
                {
                    item.SaveIdentification = "";
                    item.OnValidate();
                }
            }
        }

        [MenuItem(itemName: "PRODUKTIVKELLER/Simple Save System/Utility/Wipe Save Identifications (Active Selection(s))")]
        public static void WipeActiveSaveIdentifications()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                foreach (Saveable item in obj.GetComponentsInChildren<Saveable>(true))
                {
                    item.SaveIdentification = "";
                    item.OnValidate();
                }
            }
        }
    }
}