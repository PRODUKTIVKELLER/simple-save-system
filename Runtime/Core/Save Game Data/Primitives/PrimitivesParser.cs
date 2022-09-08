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
                case PrimitiveType.Byte:
                    return (T)(object)(Convert.ToByte(primitiveData));
                case PrimitiveType.Double:
                    return (T)(object)(Convert.ToDouble(primitiveData));
                case PrimitiveType.Uint:
                    return (T)(object)(Convert.ToUInt32(primitiveData));
                case PrimitiveType.Long:
                    return (T)(object)(Convert.ToInt64(primitiveData));
                case PrimitiveType.Ulong:
                    return (T)(object)(Convert.ToUInt64(primitiveData));
                case PrimitiveType.Short:
                    return (T)(object)(Convert.ToInt16(primitiveData));
                case PrimitiveType.Ushort:
                    return (T)(object)(Convert.ToUInt16(primitiveData));
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

            if (data.GetType() == typeof(byte))
            {
                return Convert.ToString(((int)(data)));
            }

            if (data.GetType() == typeof(double))
            {
                return Convert.ToString(((int)(data)), CultureInfo.InvariantCulture);
            }

            if (data.GetType() == typeof(uint))
            {
                return Convert.ToString(((int)(data)));
            }

            if (data.GetType() == typeof(long))
            {
                return Convert.ToString(((int)(data)));
            }

            if (data.GetType() == typeof(ulong))
            {
                return Convert.ToString(((int)(data)));
            }

            if (data.GetType() == typeof(short))
            {
                return Convert.ToString(((int)(data)));
            }

            if (data.GetType() == typeof(ushort))
            {
                return Convert.ToString(((int)(data)));
            }

            return "";
        }
    }
}
