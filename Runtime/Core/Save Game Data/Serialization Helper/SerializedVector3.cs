using System;
using System.Globalization;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializedVector3 : SerializedObject
    {
        public float dataX;
        public float dataY;
        public float dataZ;

        public Vector3 Vector3
        { get => new Vector3(dataX, dataY, dataZ); }

        public SerializedVector3()
        {

        }

        public SerializedVector3(Vector3 data)
        {
            dataX = data.x;
            dataY = data.y;
            dataZ = data.z;
        }

        public override void Deserialize(string serializedData)
        {
            dataX = Convert.ToSingle(serializedData.Split('|')[0]);
            dataY = Convert.ToSingle(serializedData.Split('|')[1]);
            dataZ = Convert.ToSingle(serializedData.Split('|')[2]);
        }

        public override string Serialize()
        {
            return Convert.ToString(dataX, CultureInfo.InvariantCulture)
                + "|"
                + Convert.ToString(dataY, CultureInfo.InvariantCulture)
                + "|"
                + Convert.ToString(dataZ, CultureInfo.InvariantCulture);
        }
    }
}
