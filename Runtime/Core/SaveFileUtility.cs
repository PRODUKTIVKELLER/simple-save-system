using Produktivkeller.SimpleSaveSystem.Configuration;
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData;
using Produktivkeller.SimpleSaveSystem.Migration;
using System;
using System.Collections.Generic;
using Produktivkeller.SimpleSaveSystem.Core.IO_Interface;
using Produktivkeller.SimpleSaveSystem.Shipped_Save_Slot;
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
        
        public static  string FileExtentionName           => SaveSettings.Get().fileExtensionName;
        private static string GameFileName                => SaveSettings.Get().fileName;
        private static string SpecialDataFolderNameSuffix => SaveSettings.Get().specialDataFolderNameSuffix;
        private static bool   DebugMode                   => SaveSettings.Get().showSaveFileUtilityLog;
        
        private static IFileReadWriter _fileReadWriter;

        public delegate void ErrorLoadingSaveGameEvent(string saveGamePath);
        public static event ErrorLoadingSaveGameEvent ErrorLoadingSaveGame;

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

        public static string DataPathLocal =>
            Application.isEditor
                ? SaveSettings.Get().fileFolderNameEditor
                : SaveSettings.Get().fileFolderName;

        private static void Log(string text)
        {
            if (DebugMode)
            {
                Debug.Log(text);
            }
        }

        private static Dictionary<int, string> cachedSavePaths;
        private static bool _isCachedSavePathsDirty;

        public static Dictionary<int, string> ObtainSavePaths()
        {
            InitializeFileReadWriterIfNecessary();

            if (cachedSavePaths != null && _isCachedSavePathsDirty == false)
            {
                return cachedSavePaths;
            }

            Dictionary<int, string> newSavePaths = new Dictionary<int, string>();

            _fileReadWriter.CreateDirectory(DataPathLocal);

            string[] savePaths = _fileReadWriter.ObtainAllSaveGameFiles();

            int pathCount = savePaths.Length;

            for (int i = 0; i < pathCount; i++)
            {
                Log($"Found save file at [{savePaths[i]}].");

                string fileName = savePaths[i].Substring(DataPathLocal.Length + GameFileName.Length + 1);

                if (int.TryParse(fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal)), out int getSlotNumber))
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
                Log($"Save file empty: {savePath}. It will be automatically removed.");
                _fileReadWriter.DeleteFile(savePath);
                return null;
            }

            SaveGame getSave = null;
            
            try
            {
                getSave = JsonUtility.FromJson<SaveGame>(data);
            }
            catch (Exception exception)
            {
                Log($"Exception occured during the parsing of the save game: [{exception.Message}].");
            }

            if (getSave != null)
            {
                getSave.OnLoad();
                return getSave;
            }

            Log($"Save file corrupted at: [{savePath}].");
            ErrorLoadingSaveGame?.Invoke(savePath);
            return null;
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

            string savePath;

            if (ObtainSavePaths().TryGetValue(slot, out savePath))
            {
                SaveGame saveGame = LoadSaveFromPath(savePath);

                if (saveGame == null)
                {
                    cachedSavePaths.Remove(slot);
                    return null;
                }

                Log($"Successful load at slot [{slot}] from cache.");
                return saveGame;
            }

            if (!createIfEmpty)
            {
                Log($"Could not load game at slot [{slot}].");
            }
            else
            {
                Log($"Creating save at slot [{slot}].");

                SaveGame saveGame = new SaveGame();

                if (ShippedSaveSlot.ExistsForSlot(slot))
                {
                    saveGame = JsonUtility.FromJson<SaveGame>(ShippedSaveSlot.GetShippedSaveGameForSlot(slot).saveGameJson);
                    saveGame.OnLoad();
                }

                saveGame.migrationVersion = MigrationMaster.GetMostRecentMigrationVersion();
                saveGame.AddCreationVersionToMigrationHistory(saveGame.migrationVersion);

                WriteSave(saveGame, slot);

                return saveGame;
            }

            return null;
        }

        public static string GetSaveGameSpecificDataFolder(int saveSlot)
        {
            return $"{DataPathLocal}/{GameFileName}{saveSlot.ToString()}{SpecialDataFolderNameSuffix}";
        }

        public static void WriteSave(SaveGame saveGame, int saveSlot)
        {
            InitializeFileReadWriterIfNecessary();

            string savePath            = $"{DataPathLocal}/{GameFileName}{saveSlot.ToString()}{FileExtentionName}";
            string specialDataSavePath = GetSaveGameSpecificDataFolder(saveSlot);

            _fileReadWriter.CreateDirectory(specialDataSavePath);
            
            if (!cachedSavePaths.ContainsKey(saveSlot))
            {
                cachedSavePaths.Add(saveSlot, savePath);
            }

            Log($"Saving game slot [{saveSlot}] to [{savePath}].");

            saveGame.OnWrite();


            _fileReadWriter.WriteText(savePath, JsonUtility.ToJson(saveGame, true));
            saveGame.WritebackAllScheduledWritebacks();

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }

        public static void DeleteSave(int slot)
        {
            InitializeFileReadWriterIfNecessary();

            string filePath = $"{DataPathLocal}/{GameFileName}{slot}{FileExtentionName}";

            if (_fileReadWriter.DeleteFile(filePath))
            {
                Log($"Successfully removed file at [{filePath}].");

                if (cachedSavePaths.ContainsKey(slot))
                {
                    cachedSavePaths.Remove(slot);
                }
            }
            else
            {
                Log($"Failed to remove file at [{filePath}].");
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