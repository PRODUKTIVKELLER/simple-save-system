using System;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedString : SerializedObject
    {
        public string data;

        public SerializedString()
        {

        }

        public SerializedString(string data)
        {
            this.data = data;
        }

        public override void Deserialize(string serializedData)
        {
            data = serializedData;
        }

        public override string Serialize()
        {
            return data;
        }
    }
}
