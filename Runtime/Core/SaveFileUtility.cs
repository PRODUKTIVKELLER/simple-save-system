using Produktivkeller.SimpleSaveSystem.Configuration;
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData;
using Produktivkeller.SimpleSaveSystem.Jobs;
using Produktivkeller.SimpleSaveSystem.Migration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Produktivkeller.SimpleSaveSystem.Core
{
    public class SaveFileUtility
    {
        // Saving with WebGL requires a seperate DLL, which is included in the plugin.
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SyncFiles();

    [DllImport("__Internal")]
    private static extern void WindowAlert(string message);
#endif

        private static Dictionary<string, SaveGame> _writebackDisabledSavegamesCache = new Dictionary<string, SaveGame>();

        private static string fileExtentionName { get { return SaveSettings.Get().fileExtensionName; } }
        private static string gameFileName { get { return SaveSettings.Get().fileName; } }

        private static bool debugMode { get { return SaveSettings.Get().showSaveFileUtilityLog; } }

        private static WritebackSaveGameJob _writebackSaveGameJob = null;

        private static string DataPath
        {
            get
            {
                return string.Format("{0}/{1}",
                    Application.persistentDataPath,
                    Application.isEditor
                        ? SaveSettings.Get().fileFolderNameEditor
                        : SaveSettings.Get().fileFolderName);
            }
        }

        private static void Log(string text)
        {
            if (debugMode)
            {
                Debug.Log(text);
            }
        }

        private static Dictionary<int, string> cachedSavePaths;
        private static bool _isCachedSavePathsDirty = false;

        public static Dictionary<int, string> ObtainSavePaths()
        {
            if (cachedSavePaths != null && _isCachedSavePathsDirty == false)
            {
                return cachedSavePaths;
            }

            Dictionary<int, string> newSavePaths = new Dictionary<int, string>();

            // Create a directory if it doesn't exist yet
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }

            string[] filePaths = Directory.GetFiles(DataPath);

            string[] savePaths = filePaths.Where(path => path.EndsWith(fileExtentionName)).ToArray();

            int pathCount = savePaths.Length;

            for (int i = 0; i < pathCount; i++)
            {
                Log(string.Format("Found save file at: {0}", savePaths[i]));

                int getSlotNumber;

                string fileName = savePaths[i].Substring(DataPath.Length + gameFileName.Length + 1);

                if (int.TryParse(fileName.Substring(0, fileName.LastIndexOf(".")), out getSlotNumber))
                {
                    newSavePaths.Add(getSlotNumber, savePaths[i]);
                }
            }

            cachedSavePaths = newSavePaths;

            return newSavePaths;
        }

        public static void MarkCachedSavePathsDirtyAsDirty()
        {
            _isCachedSavePathsDirty = true;
        }

        public static SaveGame LoadSaveFromPath(string savePath)
        {
            string data = File.ReadAllText(savePath);

            if (string.IsNullOrEmpty(data))
            {
                Log(string.Format("Save file empty: {0}. It will be automatically removed", savePath));
                File.Delete(savePath);
                return null;
            }

            SaveGame getSave = JsonUtility.FromJson<SaveGame>(data);

            if (getSave != null)
            {
                getSave.OnLoad();
                return getSave;
            }
            else
            {
                Log(string.Format("Save file corrupted: {0}", savePath));
                return null;
            }
        }

        public static int[] GetUsedSlots()
        {
            int[] saves = new int[ObtainSavePaths().Count];

            int counter = 0;

            foreach (int item in ObtainSavePaths().Keys)
            {
                saves[counter] = item;
                counter++;
            }

            return saves;
        }

        public static int GetSaveSlotCount()
        {
            return ObtainSavePaths().Count;
        }

        public static SaveGame LoadSave(int slot, bool createIfEmpty = false)
        {
            MigrationMaster.ProcessAllSavegames();

            if (slot < 0)
            {
                Debug.LogWarning("Attempted to load negative slot");
                return null;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
                SyncFiles();
#endif

            string savePath = "";

            if (SaveFileUtility.ObtainSavePaths().TryGetValue(slot, out savePath))
            {
                if (_writebackDisabledSavegamesCache.ContainsKey(savePath.Replace("\\", "/")))
                {
                    Debug.LogError("Took cached savegame, because writeback was disabled");
                    return _writebackDisabledSavegamesCache[savePath.Replace("\\", "/")];
                }

                SaveGame saveGame = LoadSaveFromPath(savePath);

                if (saveGame == null)
                {
                    cachedSavePaths.Remove(slot);
                    return null;
                }

                Log(string.Format("Succesful load at slot (from cache): {0}", slot));
                return saveGame;
            }
            else
            {
                if (!createIfEmpty)
                {
                    Log(string.Format("Could not load game at slot {0}", slot));
                }
                else
                {

                    Log(string.Format("Creating save at slot {0}", slot));

                    SaveGame saveGame = new SaveGame();

                    saveGame.migrationVersion = MigrationMaster.GetMostRecentMigrationVersion();
                    saveGame.AddCreationVersionToMigrationHistory(saveGame.migrationVersion);

                    WriteSave(saveGame, slot);

                    return saveGame;
                }

                return null;
            }
        }

        public static void WriteSave(SaveGame saveGame, int saveSlot, bool forceNoMultiThread = false)
        {
            string savePath = string.Format("{0}/{1}{2}{3}", DataPath, gameFileName, saveSlot.ToString(), fileExtentionName);

            if (SaveSettings.Get().writebackToFileDisabled)
            {
                if (_writebackDisabledSavegamesCache.ContainsKey(savePath) == false)
                {
                    _writebackDisabledSavegamesCache.Add(savePath.Replace("\\", "/"), saveGame);
                }
                _writebackDisabledSavegamesCache[savePath] = saveGame;
                return;
            }


            if (!cachedSavePaths.ContainsKey(saveSlot))
            {
                cachedSavePaths.Add(saveSlot, savePath);
            }

            Log(string.Format("Saving game slot {0} to : {1}", saveSlot.ToString(), savePath));

            saveGame.OnWrite();

            if (SaveSettings.Get().useMultiThreadedWriteback && forceNoMultiThread == false)
            {
                if (_writebackSaveGameJob != null && _writebackSaveGameJob.IsDone == false)
                {
                    Debug.Log("Skipped saving, due to running job.");
                }
                else
                {
                    Debug.Log("Started writeback job.");

                    _writebackSaveGameJob = new WritebackSaveGameJob(savePath, saveGame);

                    _writebackSaveGameJob.Start();
                }
            }
            else
            {
                File.WriteAllText(savePath, JsonUtility.ToJson(saveGame, true));
            }


#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }

        public static void DeleteSave(int slot)
        {
            string filePath = string.Format("{0}/{1}{2}{3}", DataPath, gameFileName, slot, fileExtentionName);

            if (File.Exists(filePath))
            {
                Log(string.Format("Succesfully removed file at {0}", filePath));
                File.Delete(filePath);

                if (cachedSavePaths.ContainsKey(slot))
                {
                    cachedSavePaths.Remove(slot);
                }
            }
            else
            {
                Log(string.Format("Failed to remove file at {0}", filePath));
            }

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }

        public static bool IsSlotUsed(int index)
        {
            return ObtainSavePaths().ContainsKey(index);
        }

        public static int GetAvailableSaveSlot()
        {
            int slotCount = SaveSettings.Get().maxSaveSlotCount;

            for (int i = 0; i < slotCount; i++)
            {
                if (!ObtainSavePaths().ContainsKey(i))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}