using System;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedLong : SerializedObject
    {
        public long data;

        public SerializedLong()
        {

        }

        public SerializedLong(long data)
        {
            this.data = data;
        }

        public override void Deserialize(string serializedData)
        {
            data = Convert.ToUInt32(serializedData, CultureInfo.InvariantCulture);
        }

        public override string Serialize()
        {
            return Convert.ToString(data, CultureInfo.InvariantCulture);
        }
    }
}
