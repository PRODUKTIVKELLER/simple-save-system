using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Core;
using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using Produktivkeller.SimpleSaveSystem.VersionRollback.Handler;
using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.VersionRollback
{
    public class RollbackMaster
    {
        private static bool _hasProcessedAllSavegames;

        private static RollbackHandlerSaveGame                     _rollbackHandlerSaveGame;
        private static Dictionary<string, RollbackHandlerSaveable> _rollbackHandlerSaveables;

        public static void ProcessAllSavegames()
        {
            if (_hasProcessedAllSavegames)
            {
                return;
            }

            if (SaveSettings.Get().showSaveFileUtilityLog)
            {
                Debug.Log("[SaveSystem] Performing rollback...");
            }

            _hasProcessedAllSavegames = true;

            InitializeRollbackHandlers();

            Dictionary<int, string> savePaths = SaveFileUtility.ObtainSavePaths();
            foreach (KeyValuePair<int, string> savePath in savePaths)
            {
                SaveGame saveGame = SaveFileUtility.LoadSaveFromPath(savePath.Value);
                SaveFileUtility.WriteSave(PerformRollback(saveGame), savePath.Key);
            }

            if (SaveSettings.Get().showSaveFileUtilityLog)
            {
                Debug.Log("[SaveSystem] Rollback finished");
            }
        }

        private static void InitializeRollbackHandlers()
        {
            SaveGameRollback[] saveGameRollbacks = Resources.LoadAll<SaveGameRollback>("");
            _rollbackHandlerSaveGame = new RollbackHandlerSaveGame(saveGameRollbacks);

            _rollbackHandlerSaveables = new Dictionary<string, RollbackHandlerSaveable>();
            SaveableRollback[] saveableRollbacks = Resources.LoadAll<SaveableRollback>("");
            for (int i = 0; i < saveableRollbacks.Length; i++)
            {
                string saveableComponentIdentifier = saveableRollbacks[i].saveableComponentIdentifier;
                if (_rollbackHandlerSaveables.ContainsKey(saveableComponentIdentifier) == false)
                {
                    _rollbackHandlerSaveables.Add(saveableComponentIdentifier, new RollbackHandlerSaveable(saveableComponentIdentifier));
                }

                _rollbackHandlerSaveables[saveableComponentIdentifier].AddSaveableRollback(saveableRollbacks[i]);
            }
        }

        private static SaveGame PerformRollback(SaveGame saveGame)
        {
            saveGame = _rollbackHandlerSaveGame.Rollback(saveGame);

            foreach (KeyValuePair<string, RollbackHandlerSaveable> rollbackHandlerSaveable in _rollbackHandlerSaveables)
            {
                rollbackHandlerSaveable.Value.Rollback(saveGame);
            }

            return saveGame;
        }
    }
}
