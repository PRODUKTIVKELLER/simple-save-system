using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.VersionRollback
{
    [CreateAssetMenu(fileName = "SaveGame Rollback", menuName = "PRODUKTIVKELLER/Simple Save System/SaveGame Rollback")]
    public class SaveGameRollback : ScriptableObject
    {
        public ulong saveGameVersionFrom;
        public ulong saveGameVersionTo;

        public virtual SaveGame RollbackSaveGame(SaveGame saveGame)
        {
            return saveGame;
        }
    }
}
