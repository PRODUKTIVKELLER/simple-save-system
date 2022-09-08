using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData
{
    [Serializable]
    public struct MetaData
    {
        public ulong    migrationVersion;
        public string[] migrationHistory;
        public int      gameVersion;
        public string   creationDate;
        public string   lastSaveDate;
        public string   timePlayed;
    }
}
