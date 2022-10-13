namespace Produktivkeller.SimpleSaveSystem.Core.PlayerPrefsWrapper
{
    public interface IPlayerPrefsStrategy
    {
        public void SetString(string key, string value);

        public void SetFloat(string key, float value);

        public void SetInt(string key, int value);

        public string GetString(string key);

        public float GetFloat(string key);

        public int GetInt(string key);

        public void Save();

        public void DeleteAll();

        public void DeleteKey(string key);

        public bool HasKey(string key);
    }
}
