using Produktivkeller.SimpleSaveSystem.ComponentSaveSystem.Core;
using System.IO;
using System.Linq;

namespace Produktivkeller.SimpleSaveSystem.Core.IOInterface
{
    public class DefaultFileReadWriter : IFileReadWriter
    {
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public bool DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        public string[] ObtainAllSavegameFiles()
        {
            string[] filePaths = Directory.GetFiles(SaveFileUtility.DataPath);

            string[] savePaths = filePaths.Where(path => path.EndsWith(SaveFileUtility.FileExtentionName)).ToArray();

            return savePaths;
        }

        public string ReadText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteText(string path, string text)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
