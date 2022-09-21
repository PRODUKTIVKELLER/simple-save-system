using System;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedInt : SerializedObject
    {
        public int data;

        public SerializedInt()
        {

        }

        public SerializedInt(int data)
        {
            this.data = data;
        }

        public override void Deserialize(string serializedData)
        {
            data = Convert.ToInt32(serializedData, CultureInfo.InvariantCulture);
        }

        public override string Serialize()
        {
            return Convert.ToString(data, CultureInfo.InvariantCulture);
        }
    }
}
