using Newtonsoft.Json;
using System;
using System.IO;

namespace RefereeAssistant3.Server
{
    public static class Settings
    {
        public static DirectoryInfo BaseDirectory { get; } = new DirectoryInfo($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/RefereeAssistant3Server");

        public static FileInfo SettingsFile { get; } = new FileInfo($"{BaseDirectory}/config.json");

        public static string ConnectionString { get; set; }

        public static string SheetsClientId { get; set; }

        public static string SheetsClientSecret { get; set; }

        public static void LoadSettings()
        {
            SettingsFile settings;
            if (!SettingsFile.Exists)
            {
                BaseDirectory.Create();
                settings = new SettingsFile { ConnectionString = null, Port = 5000 };
                File.WriteAllText(SettingsFile.FullName, JsonConvert.SerializeObject(settings));
            }
            else
                settings = JsonConvert.DeserializeObject<SettingsFile>(File.ReadAllText(SettingsFile.FullName));
            ConnectionString = settings.ConnectionString;
            SheetsClientId = settings.SheetsClientId;
            SheetsClientSecret = settings.SheetsClientSecret;
        }
    }
}
