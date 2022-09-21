using System;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedULong : SerializedObject
    {
        public ulong data;

        public SerializedULong()
        {

        }

        public SerializedULong(ulong data)
        {
            this.data = data;
        }

        public override void Deserialize(string serializedData)
        {
            data = Convert.ToUInt64(serializedData, CultureInfo.InvariantCulture);
        }

        public override string Serialize()
        {
            return Convert.ToString(data, CultureInfo.InvariantCulture);
        }
    }
}
