using System;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.Primitives
{
    public abstract class PrimitivesParser
    {
        public static T Parse<T>(string primitiveData, PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Bool:
                    return (T)(object)(primitiveData == "1");
                case PrimitiveType.Int:
                    return (T)(object)(Convert.ToInt32(primitiveData));
                case PrimitiveType.Float:
                    return (T)(object)(Convert.ToSingle(primitiveData, CultureInfo.InvariantCulture));
                case PrimitiveType.String:
                    return (T)(object)(primitiveData);
            }

            return default(T);
        }


        public static string ToPrimitiveData(object data)
        {
            if (data.GetType() == typeof(bool))
            {
                return ((bool)(data)) ? "1" : "0";
            }

            if (data.GetType() == typeof(int))
            {
                return Convert.ToString(((int)(data)));
            }

            if (data.GetType() == typeof(float))
            {
                return Convert.ToString(((float)(data)), CultureInfo.InvariantCulture);
            }

            if (data.GetType() == typeof(string))
            {
                return (string)data;
            }

            return "";
        }
    }
}
