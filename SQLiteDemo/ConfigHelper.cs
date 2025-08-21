using Newtonsoft.Json;
using System.IO;

namespace SQLiteDemo
{
    public static class ConfigHelper
    {
        private static readonly AppSettings _settings;

        static ConfigHelper()
        {
            // 从 appsettings.json 加载配置
            string json = File.ReadAllText("appsettings.json");
            _settings = JsonConvert.DeserializeObject<AppSettings>(json);
        }

        public static string GetMainDbConnectionString() => _settings.ConnectionStrings.MainDb;
        public static string GetTempDbConnectionString() => _settings.ConnectionStrings.TempDb;
        public static string GetEncryptionKey() => _settings.Encryption.Key;
    }
}
