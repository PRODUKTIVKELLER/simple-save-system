namespace Produktivkeller.SimpleSaveSystem.Core.PlayerPrefsWrapper
{
    public static class PlayerPrefsGeneric
    {
        private static IPlayerPrefsStrategy _playerPrefsStrategy;

        public static void SetPlayerPrefsStrategy(IPlayerPrefsStrategy playerPrefsStrategy)
        {
            _playerPrefsStrategy = playerPrefsStrategy;
        }

        private static void InitializeStrategyIfNecessary()
        {
            if (_playerPrefsStrategy != null)
            {
                return;
            }

            _playerPrefsStrategy = new PlayerPrefsDefaultStrategy();
        }

        public static void DeleteAll()
        {
            InitializeStrategyIfNecessary();

            _playerPrefsStrategy.DeleteAll();
        }

        public static void DeleteKey(string key)
        {
            InitializeStrategyIfNecessary();

            _playerPrefsStrategy.DeleteKey(key);
        }

        public static float GetFloat(string key)
        {
            InitializeStrategyIfNecessary();

            return _playerPrefsStrategy.GetFloat(key);
        }

        public static int GetInt(string key)
        {
            InitializeStrategyIfNecessary();

            return _playerPrefsStrategy.GetInt(key);
        }

        public static string GetString(string key)
        {
            InitializeStrategyIfNecessary();

            return _playerPrefsStrategy.GetString(key);
        }

        public static bool HasKey(string key)
        {
            InitializeStrategyIfNecessary();

            return _playerPrefsStrategy.HasKey(key);
        }

        public static void Save()
        {
            InitializeStrategyIfNecessary();

            _playerPrefsStrategy.Save();
        }

        public static void SetFloat(string key, float value)
        {
            InitializeStrategyIfNecessary();

            _playerPrefsStrategy.SetFloat(key, value);
        }

        public static void SetInt(string key, int value)
        {
            InitializeStrategyIfNecessary();

            _playerPrefsStrategy.SetInt(key, value);
        }

        public static void SetString(string key, string value)
        {
            InitializeStrategyIfNecessary();

            _playerPrefsStrategy.SetString(key, value);
        }
    }
}
