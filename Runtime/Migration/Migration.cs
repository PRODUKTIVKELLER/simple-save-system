using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Migration
{
    [CreateAssetMenu(fileName = "Migration", menuName = "PRODUKTIVKELLER/Simple Save System/Migration")]
    public class Migration : ScriptableObject
    {
        public ulong  version;
        public string description;

        public virtual SaveGame Migrate(SaveGame saveGame)
        {
            return saveGame;
        }

        protected void RenameID(SaveGame saveGame, string oldID, string newID)
        {
            string data = saveGame.Get("oldID");
            saveGame.Set("newID", data);
        }
    }
}
