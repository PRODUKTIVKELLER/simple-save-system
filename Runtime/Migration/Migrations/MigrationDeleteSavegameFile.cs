using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Migration.Migrations
{
    [CreateAssetMenu(fileName = "Migration Delete Savegame File", menuName = "PRODUKTIVKELLER/Simple Save System/Migration Delete Savegame File")]
    public class MigrationDeleteSavegameFile : Migration
    {
        public override SaveGame Migrate(SaveGame saveGame)
        {
            return null;
        }
    }
}
