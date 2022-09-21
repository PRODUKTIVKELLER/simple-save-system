using Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper.Debug;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SerializationHelper
{
    [Serializable]
    public class SerializationContainer
    {
        [SerializeField] private string                       className;
        [SerializeField] private string                       tag;
        [SerializeField] private string                       jsonData;
        [SerializeField] private List<SerializationContainer> childContainers;

        public SerializationContainer()
        {
            childContainers = new List<SerializationContainer>();
        }

        public SerializationContainer AddContainer()
        {
            SerializationContainer serializationContainer = new SerializationContainer();
            childContainers.Add(serializationContainer);
            return serializationContainer;
        }

        public void SetData(SerializedObject data, string tag = "")
        {
            className = data.GetType().Name;
            jsonData  = data.Serialize();
            this.tag  = tag;
        }

        /// <summary>
        /// Is the same as doing AddContainer().SetData()
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tag"></param>
        public void AddData(SerializedObject data, string tag = "")
        {
            SerializationContainer serializationContainer = AddContainer();
            serializationContainer.SetData(data, tag);
        }

        public int Count
        { get => childContainers.Count; }

        public SerializationContainer this[int i]
        {
            get { return childContainers[i]; }
            set { childContainers[i] = value; }
        }

        public SerializedObject Data
        {
            get
            {
                Type objectType = Type.GetType(typeof(SerializedObject).Namespace + "."  + className);
                SerializedObject serializedObject = (SerializedObject)Activator.CreateInstance(objectType);

                serializedObject.Deserialize(jsonData);

                return serializedObject;
            }
        }

        #region Debugging

        public void PrintToGameObject(string gameObjectName, Transform parentTransform = null)
        {
            GameObject gameObject = new GameObject("Serialization Container - " + gameObjectName);

            gameObject.transform.parent = parentTransform;

            FillDebugObjectRecursively(gameObject);
        }

        private void FillDebugObjectRecursively(GameObject gameObject)
        {
            if (jsonData != null && jsonData.Length > 0)
            {
                SerializationContainerData serializationContainerData = gameObject.AddComponent<SerializationContainerData>();
                serializationContainerData.className = className;
                serializationContainerData.tag = tag;
                serializationContainerData.jsonData = jsonData;
            }

            for (int i = 0; i < childContainers.Count; i++)
            {
                GameObject childGameObject = new GameObject("Child Container [" + i.ToString() + "]");

                childGameObject.transform.parent = gameObject.transform;

                childContainers[i].FillDebugObjectRecursively(childGameObject);
            }
        }

        #endregion
    }
}
