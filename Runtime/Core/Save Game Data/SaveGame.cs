using System;
using System.Collections.Generic;
using UnityEngine;
using Produktivkeller.SimpleSaveSystem.Migration;
using System.Globalization;

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

        [SerializeField] private MetaData           metaData;
        [SerializeField] private List<SaveableData> saveData = new List<SaveableData>();

        // Stored in dictionary for quick lookup
        [NonSerialized]
        private Dictionary<string, int> saveDataCache = new Dictionary<string, int>(StringComparer.Ordinal);

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