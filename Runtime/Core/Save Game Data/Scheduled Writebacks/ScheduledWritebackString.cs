using System.IO;

namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData.ScheduledWritebacks
{
    public class ScheduledWritebackString : ScheduledWritebackData
    {
        public string String { get; set; }

        public override void WriteToFile(string filePath)
        {
            File.WriteAllText(filePath, String);
        }
    }
}
