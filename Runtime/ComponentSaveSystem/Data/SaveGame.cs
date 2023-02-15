using System;
using System.Collections.Generic;
using UnityEngine;
using Produktivkeller.SimpleSaveSystem.Migration;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data
{
    /// <summary>
    /// Container for all saved data.
    /// Placed into a slot (separate save file)
    /// </summary>
    [Serializable]
    public class SaveGame
    {
        [Serializable]
        public struct MetaData
        {
            public ulong    version;
            public string[] migrationHistory;
            public int      gameVersion;
            public string   creationDate;
            public string   lastSaveDate;
            public string   timePlayed;
        }

        [Serializable]
        public struct Data
        {
            public string guid;
            public string data;
            public string scene;
        }

        [NonSerialized] public TimeSpan timePlayed;
        [NonSerialized] public int      gameVersion;
        [NonSerialized] public ulong    version;
        [NonSerialized] public DateTime creationDate;
        [NonSerialized] public DateTime lastSaveDate;

        [SerializeField] private MetaData   metaData;
        [SerializeField] private List<Data> saveData = new List<Data>();

        // Stored in dictionary for quick lookup
        [NonSerialized]
        private Dictionary<string, int> saveDataCache = new Dictionary<string, int>(StringComparer.Ordinal);

        [NonSerialized] private bool loaded;

        // Used to track which save ids are assigned to a specific scene
        // This makes it posible to wipe all data from a specific scene.
        [NonSerialized] private Dictionary<string, List<string>> sceneObjectIDS = new Dictionary<string, List<string>>();

        public void OnWrite()
        {
            if (creationDate == default(DateTime))
            {
                creationDate = DateTime.Now;
            }

            metaData.lastSaveDate   = lastSaveDate.ToString(CultureInfo.InvariantCulture);
            metaData.creationDate   = creationDate.ToString(CultureInfo.InvariantCulture);
            metaData.gameVersion    = gameVersion;
            metaData.timePlayed     = timePlayed.ToString();
            metaData.version        = version;
        }

        public void OnLoad()
        {
            gameVersion = metaData.gameVersion;
            version     = metaData.version;

            DateTime.TryParse(metaData.lastSaveDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out lastSaveDate);
            DateTime.TryParse(metaData.creationDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out creationDate);
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
                    AddSceneID(saveData[i].scene, saveData[i].guid);
                }
            }
        }

        public void WipeSceneData(string sceneName)
        {
            List<string> value;
            if (sceneObjectIDS.TryGetValue(sceneName, out value))
            {
                int elementCount = value.Count;
                for (int i = elementCount - 1; i >= 0; i--)
                {
                    Remove(value[i]);
                    value.RemoveAt(i);
                }
            }
            else
            {
                Debug.Log("Scene is already wiped!");
            }
        }

        public void WipeAllData()
        {
            List<string> allKeys = new List<string>(sceneObjectIDS.Keys);

            for (int i = 0; i < allKeys.Count; i++)
            {
                List<string> dataKeys = sceneObjectIDS[allKeys[i]];

                for (int j = 0; j < dataKeys.Count; j++)
                {
                    Remove(dataKeys[j]);
                }
            }
        }

        public void Remove(string id)
        {
            int saveIndex;

            if (saveDataCache.TryGetValue(id, out saveIndex))
            {
                // Zero out the string data, it will be wiped on next cache initialization.
                saveData[saveIndex] = new Data();
                saveDataCache.Remove(id);
                sceneObjectIDS.Remove(id);
            }
        }

        /// <summary>
        /// Assign any data to the given ID. If data is already present within the ID, then it will be overwritten.
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <param name="data"> Data in a string format </param>
        public void Set(string id, string data, string scene)
        {
            int saveIndex;

            if (saveDataCache.TryGetValue(id, out saveIndex))
            {
                saveData[saveIndex] = new Data()
                {
                    guid = id,
                    data = data,
                    scene = scene,
                };
            }
            else
            {
                Data newSaveData = new Data()
                {
                    guid = id,
                    data = data,
                    scene = scene,
                };

                saveData.Add(newSaveData);
                saveDataCache.Add(id, saveData.Count - 1);
                AddSceneID(scene, id);
            }
        }

        public void Set(string id, string data)
        {
            Set(id, data, "Global");
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

        /// <summary>
        /// Adds the index to a list that is identifyable by scene
        /// Makes it easy to remove save data related to a scene name.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="index"></param>
        private void AddSceneID(string scene, string id)
        {
            List<string> value;
            if (sceneObjectIDS.TryGetValue(scene, out value))
            {
                value.Add(id);
            }
            else
            {
                List<string> list = new List<string>();
                list.Add(id);
                sceneObjectIDS.Add(scene, list);
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