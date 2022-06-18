using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Migration.Handler
{
    public class MigrationHandler
    {
        private Migration[] _migrations;

        public MigrationHandler(Migration[] migrations)
        {
            _migrations = migrations;
        }

        public SaveGame Migrate(SaveGame saveGame)
        {
            ulong saveGameVersion = saveGame.version;
            ulong currentVersion = MigrationMaster.GetMostRecentMigrationVersion();

            if (SaveSettings.Get().showSaveFileUtilityLog
                && currentVersion > saveGameVersion)
            {
                Debug.Log("[SaveSystem] Migrating SaveGame from version " + saveGameVersion.ToString() + " to " + currentVersion.ToString());
            }

            for (ulong version = saveGameVersion + 1; version <= currentVersion; version++)
            {
                Migration migration = FindMigration(version);
                saveGame = migration.Migrate(saveGame);
                saveGame.AddPerformedMigrationToHistory(migration);
            }

            saveGame.version = currentVersion;
            return saveGame;
        }

        private Migration FindMigration(ulong version)
        {
            for (int i = 0; i < _migrations.Length; i++)
            {
                if (_migrations[i].version == version)
                {
                    return _migrations[i];
                }
            }

            Debug.LogError("Migration was not found for version: [" + version.ToString() + "]");
            return null;
        }
    }
}
