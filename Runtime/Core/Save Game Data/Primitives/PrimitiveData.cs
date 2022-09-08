using System;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.Primitives
{
    [Serializable]
    public struct PrimitiveData
    {
        public string guid;
        public int    type;
        public string data;
    }
}
