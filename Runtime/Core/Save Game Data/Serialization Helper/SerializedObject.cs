using System;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public abstract class SerializedObject
    {
        public abstract string Serialize();

        public abstract void Deserialize(string serializedData);

        public static implicit operator SerializedObject(string data) => new SerializedString(data);
        public static implicit operator string(SerializedObject serializedObject) => ((SerializedString)serializedObject).data;

        public static implicit operator SerializedObject(int data) => new SerializedInt(data);
        public static implicit operator int(SerializedObject serializedObject) => ((SerializedInt)serializedObject).data;

        public static implicit operator SerializedObject(byte data) => new SerializedByte(data);
        public static implicit operator byte(SerializedObject serializedObject) => ((SerializedByte)serializedObject).data;

        public static implicit operator SerializedObject(float data) => new SerializedFloat(data);
        public static implicit operator float(SerializedObject serializedObject) => ((SerializedFloat)serializedObject).data;

        public static implicit operator SerializedObject(long data) => new SerializedLong(data);
        public static implicit operator long(SerializedObject serializedObject) => ((SerializedLong)serializedObject).data;

        public static implicit operator SerializedObject(ulong data) => new SerializedULong(data);
        public static implicit operator ulong(SerializedObject serializedObject) => ((SerializedULong)serializedObject).data;

        public static implicit operator SerializedObject(Vector3 data) => new SerializedVector3(data);
        public static implicit operator Vector3(SerializedObject serializedObject) => ((SerializedVector3)serializedObject).Vector3;
    }
}
