using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.VersionRollback.Handler
{
    public class RollbackHandlerSaveable
    {
        private string                 _saveableComponentIdentifier;
        private List<SaveableRollback> _saveableRollbacks;

        public RollbackHandlerSaveable(string saveableComponentIdentifier)
        {
            _saveableComponentIdentifier = saveableComponentIdentifier;
            _saveableRollbacks           = new List<SaveableRollback>();
        }

        public void AddSaveableRollback(SaveableRollback saveableRollback)
        {
            _saveableRollbacks.Add(saveableRollback);
        }

        public void Rollback(SaveGame saveGame)
        {
            ulong saveGameVersion = saveGame.GetVersion(_saveableComponentIdentifier);
            ulong currentVersion  = RetrieveMostRecentVersion();

            if (SaveSettings.Get().showSaveFileUtilityLog)
            {
                Debug.Log("[SaveSystem] Rolling back Saveable from version " + saveGameVersion.ToString() + " to " + currentVersion.ToString());
            }

            string saveableData = saveGame.Get(_saveableComponentIdentifier);
            for (ulong version = saveGameVersion; version < currentVersion; version++)
            {
                SaveableRollback saveableRollback = FindSaveableRollback(version, version + 1);
                saveableData = saveableRollback.RollbackSaveGame(saveableData);
            }

            saveGame.Set(_saveableComponentIdentifier, saveableData, "Global", currentVersion);
        }

        private SaveableRollback FindSaveableRollback(ulong versionFrom, ulong versionTo)
        {
            for (int i = 0; i < _saveableRollbacks.Count; i++)
            {
                if (_saveableRollbacks[i].versionFrom == versionFrom
                    && _saveableRollbacks[i].versionTo == versionTo)
                {
                    return _saveableRollbacks[i];
                }
            }

            Debug.LogError("SaveableRollback was not found for version port: [" + versionFrom.ToString() + "] -> [" + versionTo.ToString() + "]");
            return null;
        }

        private ulong RetrieveMostRecentVersion()
        {
            ulong mostRecentVersion = 0;
            for (int i = 0; i < _saveableRollbacks.Count; i++)
            {
                mostRecentVersion = _saveableRollbacks[i].versionTo > mostRecentVersion ? _saveableRollbacks[i].versionTo : mostRecentVersion;
            }

            return mostRecentVersion;
        }
    }
}
