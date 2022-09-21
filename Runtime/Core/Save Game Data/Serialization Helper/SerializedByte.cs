using System;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedByte : SerializedObject
    {
        public byte data;

        public SerializedByte()
        {

        }

        public SerializedByte(byte data)
        {
            this.data = data;
        }

        public override void Deserialize(string serializedData)
        {
            data = Convert.ToByte(serializedData, CultureInfo.InvariantCulture);
        }

        public override string Serialize()
        {
            return Convert.ToString(data, CultureInfo.InvariantCulture);
        }
    }
}
