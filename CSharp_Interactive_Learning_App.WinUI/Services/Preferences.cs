using Windows.Storage;

namespace CSharp_Interactive_Learning_App.WinUI.Services
{
    public static class Preferences
    {
        private static readonly ApplicationDataContainer _settings =
            ApplicationData.Current.LocalSettings;

        public static bool ContainsKey(string key)
        {
            return _settings.Values.ContainsKey(key);
        }

        public static void Clear()
        {
            _settings.Values.Clear();
        }

        public static string? Get(string key, string? defaultValue = null)
        {
            return _settings.Values.TryGetValue(key, out var value)
                ? value?.ToString()
                : defaultValue;
        }

        public static int Get(string key, int defaultValue = 0)
        {
            return _settings.Values.TryGetValue(key, out var value)
                ? (int)value
                : defaultValue;
        }

        public static double Get(string key, double defaultValue = 0)
        {
            return _settings.Values.TryGetValue(key, out var value)
                ? (double)value
                : defaultValue;
        }

        public static float Get(string key, float defaultValue = 0)
        {
            return _settings.Values.TryGetValue(key, out var value)
                ? (float)value
                : defaultValue;
        }

        public static bool Get(string key, bool defaultValue = false)
        {
            return _settings.Values.TryGetValue(key, out var value)
                ? (bool)value
                : defaultValue;
        }

        public static decimal Get(string key, decimal defaultValue = 0)
        {
            return _settings.Values.TryGetValue(key, out var value) 
                ? (decimal)value 
                : defaultValue;
        }

        public static long Get(string key, long defaultValue = 0)
        {
            return _settings.Values.TryGetValue(key, out var value)
                ? (long)value
                : defaultValue;
        }

        public static void Remove(string key)
        {
            _settings.Values.Remove(key);
        }

        public static void Set(string key, string value)
        {
            _settings.Values[key] = value;
        }

        public static void Set(string key, int value)
        {
            _settings.Values[key] = value;
        }

        public static void Set(string key, double value)
        {
            _settings.Values[key] = value;
        }

        public static void Set(string key, float value)
        {
            _settings.Values[key] = value;
        }

        public static void Set(string key, bool value)
        {
            _settings.Values[key] = value;
        }

        public static void Set(string key, decimal value)
        {
            _settings.Values[key] = value;
        }

        public static void Set(string key, long value)
        {
            _settings.Values[key] = value;
        }
    }
}
