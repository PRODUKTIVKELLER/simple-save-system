using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.VersionRollback.Handler
{
    public class RollbackHandlerSaveGame
    {
        private SaveGameRollback[] _saveGameRollbacks;

        public RollbackHandlerSaveGame(SaveGameRollback[] saveGameRollbacks)
        {
            _saveGameRollbacks = saveGameRollbacks;
        }

        public SaveGame Rollback(SaveGame saveGame)
        {
            ulong saveGameVersion = saveGame.version;
            ulong currentVersion = SaveSettings.Get().saveGameVersion;

            if (SaveSettings.Get().showSaveFileUtilityLog
                && currentVersion > saveGameVersion)
            {
                Debug.Log("[SaveSystem] Rolling back SaveGame from version " + saveGameVersion.ToString() + " to " + currentVersion.ToString());
            }

            for (ulong version = saveGameVersion; version < currentVersion; version++)
            {
                SaveGameRollback saveGameRollback = FindSaveGameRollback(version, version + 1);
                saveGame = saveGameRollback.RollbackSaveGame(saveGame);
            }

            saveGame.version = currentVersion;
            return saveGame;
        }

        private SaveGameRollback FindSaveGameRollback(ulong versionFrom, ulong versionTo)
        {
            for (int i = 0; i < _saveGameRollbacks.Length; i++)
            {
                if (_saveGameRollbacks[i].saveGameVersionFrom == versionFrom
                    && _saveGameRollbacks[i].saveGameVersionTo == versionTo)
                {
                    return _saveGameRollbacks[i];
                }
            }

            Debug.LogError("SaveGameRollback was not found for version port: [" + versionFrom.ToString() + "] -> [" + versionTo.ToString() + "]");
            return null;
        }
    }
}
