using System.IO;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData.ScheduledWritebacks
{
    public class ScheduledWritebackBytes : ScheduledWritebackData
    {
        public byte[] Bytes { get; set; }

        public override void WriteToFile(string filePath)
        {
            File.WriteAllBytes(filePath, Bytes);
        }
    }
}
