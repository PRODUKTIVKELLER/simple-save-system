using System.Collections.Generic;
using System.Linq;
using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using Produktivkeller.SimpleSaveSystem.Core.IOInterface;
using Produktivkeller.SimpleSaveSystem.Migration;
using UnityEngine;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Core
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

        public static string FileExtentionName { get { return SaveSettings.Get().fileExtensionName; } }
        private static string gameFileName { get { return SaveSettings.Get().fileName; } }

        private static bool debugMode { get { return SaveSettings.Get().showSaveFileUtilityLog; } }
        private static IFileReadWriter _fileReadWriter;

        public static void SetFileReadWriter(IFileReadWriter fileReadWriter)
        {
            _fileReadWriter = fileReadWriter;
        }

        private static void InitializeFileReadWriterIfNecessary()
        {
            if (_fileReadWriter != null)
            {
                return;
            }

            _fileReadWriter = new DefaultFileReadWriter();
        }

        public static string DataPathLocal
        {
            get
            {
                return Application.isEditor
                        ? SaveSettings.Get().fileFolderNameEditor
                        : SaveSettings.Get().fileFolderName;
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
            InitializeFileReadWriterIfNecessary();

            if (cachedSavePaths != null && _isCachedSavePathsDirty == false)
            {
                return cachedSavePaths;
            }

            Dictionary<int, string> newSavePaths = new Dictionary<int, string>();

            _fileReadWriter.CreateDirectory(SaveFileUtility.DataPathLocal);

            string[] savePaths = _fileReadWriter.ObtainAllSavegameFiles();

            int pathCount = savePaths.Length;

            for (int i = 0; i < pathCount; i++)
            {
                Log(string.Format("Found save file at: {0}", savePaths[i]));

                int getSlotNumber;

                string fileName = savePaths[i].Substring(DataPathLocal.Length + gameFileName.Length + 1);

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
            InitializeFileReadWriterIfNecessary();

            string data = _fileReadWriter.ReadText(savePath);

            if (string.IsNullOrEmpty(data))
            {
                Log(string.Format("Save file empty: {0}. It will be automatically removed", savePath));
                _fileReadWriter.DeleteFile(savePath);
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

                    if (ShippedSaveGame.ExistsShippedSaveGameForSlot(slot))
                    {
                        saveGame = JsonUtility.FromJson<SaveGame>(ShippedSaveGame.GetShippedSaveGameForSlot(slot).saveGameJson);
                    }

                    saveGame.version = MigrationMaster.GetMostRecentMigrationVersion();
                    saveGame.AddCreationVersionToMigrationHistory(saveGame.version);

                    WriteSave(saveGame, slot);

                    return saveGame;
                }

                return null;
            }
        }

        public static void WriteSave(SaveGame saveGame, int saveSlot)
        {
            InitializeFileReadWriterIfNecessary();

            string savePath = string.Format("{0}/{1}{2}{3}", DataPathLocal, gameFileName, saveSlot.ToString(), FileExtentionName);

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

            _fileReadWriter.WriteText(savePath, JsonUtility.ToJson(saveGame, SaveSettings.Get().useJsonPrettyPrint));

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }

        public static void DeleteSave(int slot)
        {
            string filePath = string.Format("{0}/{1}{2}{3}", DataPathLocal, gameFileName, slot, FileExtentionName);

            if (_fileReadWriter.DeleteFile(filePath))
            {
                Log(string.Format("Succesfully removed file at {0}", filePath));

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