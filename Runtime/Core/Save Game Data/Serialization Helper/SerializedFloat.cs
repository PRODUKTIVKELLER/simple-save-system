using System;
using System.Globalization;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedFloat : SerializedObject
    {
        public float data;

        public SerializedFloat()
        {

        }

        public SerializedFloat(float data)
        {
            this.data = data;
        }

        public override void Deserialize(string serializedData)
        {
            data = Convert.ToSingle(serializedData, CultureInfo.InvariantCulture);
        }

        public override string Serialize()
        {
            return Convert.ToString(data, CultureInfo.InvariantCulture);
        }
    }
}
