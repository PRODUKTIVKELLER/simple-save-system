using Produktivkeller.SimpleSaveSystem.Core.SaveGameData.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        [NonSerialized] public int      gameVersion;
        [NonSerialized] public ulong    version;
        [NonSerialized] public DateTime creationDate;
        [NonSerialized] public DateTime lastSaveDate;

        [SerializeField] private MetaData            metaData;
        [SerializeField] private List<SaveableData>  saveData      = new List<SaveableData>();
        [SerializeField] private List<PrimitiveData> primitiveData = new List<PrimitiveData>();

        // Stored in dictionary for quick lookup
        [NonSerialized]
        private Dictionary<string, int> saveDataCache      = new Dictionary<string, int>(StringComparer.Ordinal);
        private Dictionary<string, int> primitiveDataCache = new Dictionary<string, int>(StringComparer.Ordinal);

        [NonSerialized] private bool loaded;

        public void OnWrite()
        {
            if (creationDate == default(DateTime))
            {
                creationDate = DateTime.Now;
            }

            metaData.lastSaveDate   = lastSaveDate.ToString();
            metaData.creationDate   = creationDate.ToString();
            metaData.gameVersion    = gameVersion;
            metaData.timePlayed     = timePlayed.ToString();
            metaData.version        = version;
        }

        public void OnLoad()
        {
            gameVersion = metaData.gameVersion;
            version     = metaData.version;

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
                    type = PrimitiveTypeExtensions.FromType(data.GetType()).ToInt(),
                };
            }
            else
            {
                PrimitiveData newPrimitiveData = new PrimitiveData()
                {
                    data = PrimitivesParser.ToPrimitiveData(data),
                    guid = id,
                    type = PrimitiveTypeExtensions.FromType(data.GetType()).ToInt(),
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
                return PrimitivesParser.Parse<T>(primitiveData[saveIndex].data, PrimitiveTypeExtensions.FromInt(primitiveData[saveIndex].type));
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

            string[] newMigrationHistory = new string[metaData.migrationHistory.Length + 1];
            for (int i = 0; i < metaData.migrationHistory.Length; i++)
            {
                newMigrationHistory[i] = metaData.migrationHistory[i];
            }
            newMigrationHistory[newMigrationHistory.Length - 1] = "[" + migration.version.ToString() + "] "
                + "[" + DateTime.UtcNow.ToString("G", CultureInfo.GetCultureInfo("en-US")) + "] "
                + migration.description;

            metaData.migrationHistory = newMigrationHistory;
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
    }
}