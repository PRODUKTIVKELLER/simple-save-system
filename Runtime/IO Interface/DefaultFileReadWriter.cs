using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Core;
using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Produktivkeller.SimpleSaveSystem.Core.IOInterface
{
    public class DefaultFileReadWriter : IFileReadWriter
    {
        public void CreateDirectory(string path)
        {
            string fullPath = AddApplicationPersistentDataPathToString(path);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public bool DeleteFile(string path)
        {
            string fullPath = AddApplicationPersistentDataPathToString(path);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }

        public string[] ObtainAllSavegameFiles()
        {
            string[] filePaths = Directory.GetFiles(AddApplicationPersistentDataPathToString(SaveFileUtility.DataPathLocal));

            string[] savePaths = filePaths.Where(path => path.EndsWith(SaveFileUtility.FileExtentionName)).ToArray();

            for (int i = 0; i < savePaths.Length; i++)
            {
                savePaths[i] = RemoveApplicationPersistentDataPathFromString(savePaths[i]);
            }

            return savePaths;
        }

        public string ReadText(string path)
        {
            string fullPath = AddApplicationPersistentDataPathToString(path);
            string data = "";

            using (var reader = new BinaryReader(File.Open(fullPath, FileMode.Open)))
            {
                data = reader.ReadString();
            }

            return data;
        }

        public void WriteText(string path, string text, bool forceInstantWriteback)
        {
            string fullPath = AddApplicationPersistentDataPathToString(path);

            using (var writer = new BinaryWriter(File.Open(fullPath, FileMode.Create)))
            {
                var jsonString = text;

                writer.Write(jsonString);
            }
        }

        private string RemoveApplicationPersistentDataPathFromString(string text)
        {
            string textWithoutPersDataPath = text.Replace("\\", "/").
                    Replace(Application.persistentDataPath.Replace("\\", "/"), "");

            if (textWithoutPersDataPath.StartsWith("/"))
            {
                textWithoutPersDataPath = textWithoutPersDataPath.Substring(1);
            }

            return textWithoutPersDataPath;
        }

        private string AddApplicationPersistentDataPathToString(string text)
        {
            return string.Format("{0}/{1}",
                    Application.persistentDataPath, text);
        }
    }
}
