using Produktivkeller.SimpleSaveSystem.Core;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Migration
{
    [CreateAssetMenu(fileName = "Migration", menuName = "PRODUKTIVKELLER/Simple Save System/Migration")]
    public class Migration : ScriptableObject
    {
        public ulong  version;
        public string description;

        /// <summary>
        /// Performs any migration on the given SaveGame, and returns the migrated SaveGame.
        /// If null is returned, the file of the concerned SaveGame is deleted.
        /// </summary>
        /// <param name="saveGame"></param>
        /// <returns></returns>
        public virtual SaveGame Migrate(SaveGame saveGame)
        {
            return saveGame;
        }

        protected void RenameID(SaveGame saveGame, string oldID, string newID)
        {
            string data = saveGame.Get(oldID);
            saveGame.Set(newID, data);
        }
    }
}
