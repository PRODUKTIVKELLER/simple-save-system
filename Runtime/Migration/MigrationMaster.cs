using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Core;
using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using Produktivkeller.SimpleSaveSystem.Migration.Handler;
using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Migration
{
    public class MigrationMaster
    {
        private static bool _hasProcessedAllSavegames;

        private static MigrationHandler _migrationHandler;

        public static void ProcessAllSavegames()
        {
            if (_hasProcessedAllSavegames)
            {
                return;
            }

            if (SaveSettings.Get().showSaveFileUtilityLog)
            {
                Debug.Log("[SaveSystem] Performing migration...");
            }

            _hasProcessedAllSavegames = true;

            InitializeMigrationHandlers();

            List<int> slotsScheduledToDeletion = new List<int>();

            Dictionary<int, string> savePaths = SaveFileUtility.ObtainSavePaths();
            foreach (KeyValuePair<int, string> savePath in savePaths)
            {
                SaveGame saveGame = SaveFileUtility.LoadSaveFromPath(savePath.Value);

                SaveGame migratedSavegame = PerformMigration(saveGame);

                if (migratedSavegame != null)
                {
                    SaveFileUtility.WriteSave(migratedSavegame, savePath.Key);
                }
                else
                {
                    slotsScheduledToDeletion.Add(savePath.Key);
                }
            }

            for (int i = 0; i < slotsScheduledToDeletion.Count; i++)
            {
                SaveFileUtility.DeleteSave(slotsScheduledToDeletion[i]);
            }

            if (SaveSettings.Get().showSaveFileUtilityLog)
            {
                Debug.Log("[SaveSystem] Migration finished");
            }
        }

        private static void InitializeMigrationHandlers()
        {
            Migration[] migrations = Resources.LoadAll<Migration>("");

            if (!AreMigrationsValid(migrations))
            {
                Debug.LogError("Invalid migrations. Make sure no migration version number is duplicated, and all numbers"
                    + "go successively from 1 upwards");
                return;
            }

            _migrationHandler = new MigrationHandler(migrations);
        }

        private static SaveGame PerformMigration(SaveGame saveGame)
        {
            saveGame = _migrationHandler.Migrate(saveGame);

            return saveGame;
        }

        private static bool AreMigrationsValid(Migration[] saveGameRollbacks)
        {
            for (int i = 0; i < saveGameRollbacks.Length; i++)
            {
                ulong migrationVersion = saveGameRollbacks[i].version;
                bool versionNumberValid = false;
                for (int j = 0; j < saveGameRollbacks.Length; j++)
                {
                    if (migrationVersion == (ulong)(j + 1))
                    {
                        versionNumberValid = true;
                        break;
                    }
                }

                if (!versionNumberValid)
                {
                    return false;
                }
            }

            return true;
        }

        public static ulong GetMostRecentMigrationVersion()
        {
            Migration[] migrations = Resources.LoadAll<Migration>("");

            ulong mostRecentVersion = 0;
            for (int i = 0; i < migrations.Length; i++)
            {
                mostRecentVersion = migrations[i].version > mostRecentVersion ? migrations[i].version : mostRecentVersion;
            }
            return mostRecentVersion;
        }
    }
}
