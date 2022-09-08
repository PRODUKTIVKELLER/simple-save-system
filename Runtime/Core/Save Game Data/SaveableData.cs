using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData
{
    [Serializable]
    public struct SaveableData
    {
        public string guid;
        public string data;
    }
}
