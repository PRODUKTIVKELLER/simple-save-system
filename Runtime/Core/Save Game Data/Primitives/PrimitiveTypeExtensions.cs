using System;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.Primitives
{
    public static class PrimitiveTypeExtensions
    {
        public static int ToInt(this PrimitiveType primitiveType)
        {
            return (int)primitiveType;
        }

        public static PrimitiveType FromInt(int typeInt)
        {
            return (PrimitiveType)typeInt;
        }

        public static PrimitiveType FromType(Type type)
        {
            if (type == typeof(bool))
            {
                return PrimitiveType.Bool;
            }

            if (type == typeof(int))
            {
                return PrimitiveType.Int;
            }

            if (type == typeof(float))
            {
                return PrimitiveType.Float;
            }

            if (type == typeof(string))
            {
                return PrimitiveType.String;
            }

            return PrimitiveType.None;
        }
    }
}
