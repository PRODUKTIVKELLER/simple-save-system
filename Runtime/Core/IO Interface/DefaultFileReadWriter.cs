using System.IO;
using System.Linq;

namespace Produktivkeller.SimpleSaveSystem.Core.IO_Interface
{
    public class DefaultFileReadWriter : IFileReadWriter
    {
        public static string applicationPersistentDataPath;

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

        public string[] ObtainAllSaveGameFiles()
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
            return File.ReadAllText(AddApplicationPersistentDataPathToString(path));
        }

        public void WriteText(string path, string text)
        {
            string fullPath = AddApplicationPersistentDataPathToString(path);

            File.WriteAllText(fullPath, text);
        }

        private string RemoveApplicationPersistentDataPathFromString(string text)
        {
            string result = text.Replace("\\", "/").
                    Replace(applicationPersistentDataPath.Replace("\\", "/"), "");

            if (result.StartsWith("/"))
            {
                result = result[1..];
            }

            return result;
        }

        private string AddApplicationPersistentDataPathToString(string text)
        {
            return $"{applicationPersistentDataPath}/{text}";
        }
    }
}
