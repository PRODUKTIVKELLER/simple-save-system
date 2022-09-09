using System;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.Primitives
{
    [Serializable]
    public struct PrimitiveData
    {
        public string        guid;
        public PrimitiveType type;
        public string        data;
    }
}
