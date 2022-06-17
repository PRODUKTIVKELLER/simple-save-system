using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.VersionRollback
{
    [CreateAssetMenu(fileName = "Saveable Rollback", menuName = "PRODUKTIVKELLER/Simple Save System/Saveable Rollback")]
    public class SaveableRollback : ScriptableObject
    {
        public ulong  versionFrom;
        public ulong  versionTo;
        public string saveableComponentIdentifier;

        public virtual string RollbackSaveGame(string data)
        {
            return data;
        }
    }
}
