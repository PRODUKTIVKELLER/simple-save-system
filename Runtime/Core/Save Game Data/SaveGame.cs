using Produktivkeller.SimpleSaveSystem.Core.SaveGameData.Primitives;
using Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData.ScheduledWritebacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData
{
    /// <summary>
    /// Container for all saved data.
    /// Placed into a slot (separate save file)
    /// </summary>
    [Serializable]
    public class SaveGame
    {
        [NonSerialized] public TimeSpan timePlayed;
        [NonSerialized] public string   gameVersion;
        [NonSerialized] public ulong    migrationVersion;
        [NonSerialized] public DateTime creationDate;
        [NonSerialized] public DateTime lastSaveDate;

        [SerializeField] private MetaData            metaData;
        [SerializeField] private List<SaveableData>  saveData;
        [SerializeField] private List<PrimitiveData> primitiveData;

        // Stored in dictionary for quick lookup
        [NonSerialized]
        private Dictionary<string, int> saveDataCache;
        private Dictionary<string, int> primitiveDataCache;

        [NonSerialized] private bool loaded;

        [NonSerialized] private Dictionary<string, ScheduledWritebackData> _scheduledWritebackData;

        public SaveGame()
        {
            saveData      = new List<SaveableData>();
            primitiveData = new List<PrimitiveData>();

            saveDataCache      = new Dictionary<string, int>(StringComparer.Ordinal);
            primitiveDataCache = new Dictionary<string, int>(StringComparer.Ordinal);

            _scheduledWritebackData = new Dictionary<string, ScheduledWritebackData>();
        }

        public void OnWrite()
        {
            if (creationDate == default(DateTime))
            {
                creationDate = DateTime.Now;
            }

            metaData.lastSaveDate     = lastSaveDate.ToString();
            metaData.creationDate     = creationDate.ToString();
            metaData.timePlayed       = timePlayed.ToString();
            metaData.migrationVersion = migrationVersion;

            AddGameVersionToHistory(Application.version);
        }

        public void OnLoad()
        {
            migrationVersion = metaData.migrationVersion;

            DateTime.TryParse(metaData.lastSaveDate, out lastSaveDate);
            DateTime.TryParse(metaData.creationDate, out creationDate);
            TimeSpan.TryParse(metaData.timePlayed, out timePlayed);

            if (saveData.Count > 0)
            {
                // Clear all empty data on load.
                int dataCount = saveData.Count;
                for (int i = dataCount - 1; i >= 0; i--)
                {
                    if (string.IsNullOrEmpty(saveData[i].data))
                        saveData.RemoveAt(i);
                }

                for (int i = 0; i < saveData.Count; i++)
                {
                    saveDataCache.Add(saveData[i].guid, i);
                }

                for (int i = 0; i < primitiveData.Count; i++)
                {
                    primitiveDataCache.Add(primitiveData[i].guid, i);
                }
            }
        }

        public void AddScheduledFileData(string filePath, byte[] data)
        {
            if (!_scheduledWritebackData.ContainsKey(filePath))
            {
                _scheduledWritebackData.Add(filePath, new ScheduledWritebackBytes()
                {
                    Bytes = data
                });
            }

            ((ScheduledWritebackBytes)_scheduledWritebackData[filePath]).Bytes = data;
        }

        public void AddScheduledFileData(string filePath, string data)
        {
            if (!_scheduledWritebackData.ContainsKey(filePath))
            {
                _scheduledWritebackData.Add(filePath, new ScheduledWritebackString()
                {
                    String = data
                });
            }

            ((ScheduledWritebackString)_scheduledWritebackData[filePath]).String = data;
        }

        public void WritebackAllScheduledWritebacks()
        {
            foreach (KeyValuePair<string, ScheduledWritebackData> keyValuePair in _scheduledWritebackData)
            {
                keyValuePair.Value.WriteToFile(keyValuePair.Key);
            }
        }

        public void WipeAllData()
        {
            foreach (KeyValuePair<string, int> keyValuePair in saveDataCache)
            {
                Remove(keyValuePair.Key);
            }
        }

        public void Remove(string id)
        {
            int saveIndex;

            if (saveDataCache.TryGetValue(id, out saveIndex))
            {
                // Zero out the string data, it will be wiped on next cache initialization.
                saveData[saveIndex] = new SaveableData();
                saveDataCache.Remove(id);
            }
        }

        public void RemovePrimitive(string id)
        {
            int saveIndex;

            if (primitiveDataCache.TryGetValue(id, out saveIndex))
            {
                // Zero out the string data, it will be wiped on next cache initialization.
                primitiveData[saveIndex] = new PrimitiveData();
                primitiveDataCache.Remove(id);
            }
        }

        public void SetPrimitive(string id, object data)
        {
            int saveIndex;

            if (primitiveDataCache.TryGetValue(id, out saveIndex))
            {
                primitiveData[saveIndex] = new PrimitiveData()
                {
                    data = PrimitivesParser.ToPrimitiveData(data),
                    guid = id,
                    type = PrimitiveTypeExtensions.FromType(data.GetType()),
                };
            }
            else
            {
                PrimitiveData newPrimitiveData = new PrimitiveData()
                {
                    data = PrimitivesParser.ToPrimitiveData(data),
                    guid = id,
                    type = PrimitiveTypeExtensions.FromType(data.GetType()),
                };

                primitiveData.Add(newPrimitiveData);
                primitiveDataCache.Add(id, primitiveData.Count - 1);
            }
        }

        public T GetPrimitive<T>(string id)
        {
            int saveIndex;

            if (primitiveDataCache.TryGetValue(id, out saveIndex))
            {
                return PrimitivesParser.Parse<T>(primitiveData[saveIndex].data, primitiveData[saveIndex].type);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Assign any data to the given ID. If data is already present within the ID, then it will be overwritten.
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <param name="data"> Data in a string format </param>
        public void Set(string id, string data)
        {
            int saveIndex;

            if (saveDataCache.TryGetValue(id, out saveIndex))
            {
                saveData[saveIndex] = new SaveableData()
                {
                    guid = id,
                    data = data,
                };
            }
            else
            {
                SaveableData newSaveData = new SaveableData()
                {
                    guid = id,
                    data = data,
                };

                saveData.Add(newSaveData);
                saveDataCache.Add(id, saveData.Count - 1);
            }
        }

        /// <summary>
        /// Returns any data stored based on a identifier
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <returns></returns>
        public string Get(string id)
        {
            int saveIndex;

            if (saveDataCache.TryGetValue(id, out saveIndex))
            {
                return saveData[saveIndex].data;
            }
            else
            {
                return null;
            }
        }

        public void AddPerformedMigrationToHistory(Produktivkeller.SimpleSaveSystem.Migration.Migration migration)
        {
            if (metaData.migrationHistory == null)
            {
                metaData.migrationHistory = new string[0];
            }

            metaData.migrationHistory = AddElementToStringArray("[" + migration.version.ToString() + "] "
                + "[" + DateTime.UtcNow.ToString("G", CultureInfo.GetCultureInfo("en-US")) + "] "
                + migration.description,
                metaData.migrationHistory);
        }

        public void AddCreationVersionToMigrationHistory(ulong creationVersion)
        {
            metaData.migrationHistory = new string[]
            {
                "[" + creationVersion.ToString() + "] "
                + "[" + DateTime.UtcNow.ToString("G", CultureInfo.GetCultureInfo("en-US")) + "] "
                + "Created initial savegame"
            };
        }

        private void AddGameVersionToHistory(string gameVersion)
        {
            if (metaData.gameVersionHistory == null)
            {
                metaData.gameVersionHistory = new string[0];
            }

            if (metaData.gameVersionHistory.Contains(gameVersion))
            {
                return;
            }

            metaData.gameVersionHistory = AddElementToStringArray(gameVersion, metaData.gameVersionHistory);
        }

        #region Tags

        private void InitializeTags()
        {
            if (metaData.tags != null)
            {
                return;
            }

            metaData.tags = new string[0];
        }

        public void AddTag(string tag)
        {
            InitializeTags();

            if (metaData.tags.Contains(tag))
            {
                return;
            }

            metaData.tags = AddElementToStringArray(tag, metaData.tags);
        }

        public bool ContainsTag(string tag)
        {
            InitializeTags();

            return metaData.tags.Contains(tag);
        }

        public string[] GetAllTags()
        {
            InitializeTags();

            return metaData.tags;
        }

        /// <summary>
        /// Returns, whether the specified tag has been deleted.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool DeleteTag(string tag)
        {
            InitializeTags();

            if (!metaData.tags.Contains(tag))
            {
                return false;
            }

            metaData.tags = RemoveElementFromStringArray(tag, metaData.tags);
            return true;
        }

        public void ClearAllTags()
        {
            metaData.tags = new string[0];
        }

        #endregion

        #region Boilerplate

        private static string[] AddElementToStringArray(string element, string[] array)
        {
            string[] newArray = new string[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[newArray.Length - 1] = element;
            return newArray;
        }

        private static string[] RemoveElementFromStringArray(string element, string[] array)
        {
            if (!array.Contains(element))
            {
                return array;
            }

            List<string> newArrayList = new List<string>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != element)
                {
                    newArrayList.Add(element);
                }
            }

            return newArrayList.ToArray();
        }

        #endregion
    }
}