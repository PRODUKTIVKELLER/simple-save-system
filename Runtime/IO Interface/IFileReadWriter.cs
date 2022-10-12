namespace Produktivkeller.SimpleSaveSystem.Core.IOInterface
{
    public interface IFileReadWriter
    {
        public void WriteText(string path, string text);

        public string ReadText(string path);

        public string[] ObtainAllSavegameFiles();

        public void CreateDirectory(string path);

        public bool DeleteFile(string path);
    }
}
