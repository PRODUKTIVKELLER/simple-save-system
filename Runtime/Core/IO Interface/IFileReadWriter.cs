namespace Produktivkeller.SimpleSaveSystem.Core.IO_Interface
{
    public interface IFileReadWriter
    {
        public void WriteText(string path, string text);

        public string ReadText(string path);

        public string[] ObtainAllSaveGameFiles();

        public void CreateDirectory(string path);

        public bool DeleteFile(string path);
    }
}
