namespace Produktivkeller.SimpleSaveSystem.Core.SaveGameData.SpecialData.ScheduledWritebacks
{
    public abstract class ScheduledWritebackData
    {
        public abstract void WriteToFile(string filePath);
    }
}
