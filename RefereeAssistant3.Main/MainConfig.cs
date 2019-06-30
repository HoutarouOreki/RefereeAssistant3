using Newtonsoft.Json;
using System.IO;

namespace RefereeAssistant3.Main
{
    public static class MainConfig
    {
        public static string APIKey;
        public static string IRCUsername;
        public static string IRCPassword;
        public static string ServerURL;

        private static FileInfo mainConfigFile => new FileInfo($"{Utilities.GetBaseDirectory()}/mainConfig.json");

        public static void Load()
        {
            MainConfigFile settings;
            if (!mainConfigFile.Exists)
            {
                Utilities.GetBaseDirectory().Create();
                settings = new MainConfigFile { APIKey = null, IRCPassword = null, IRCUsername = null, ServerURL = null };
                File.WriteAllText(mainConfigFile.FullName, JsonConvert.SerializeObject(settings));
            }
            else
                settings = JsonConvert.DeserializeObject<MainConfigFile>(File.ReadAllText(mainConfigFile.FullName));
            APIKey = settings.APIKey;
            IRCUsername = settings.IRCUsername;
            IRCPassword = settings.IRCPassword;
            ServerURL = settings.ServerURL;
        }

        public static void Save() => File.WriteAllText(mainConfigFile.FullName, JsonConvert.SerializeObject(new MainConfigFile
        {
            APIKey = APIKey,
            IRCUsername = IRCUsername,
            IRCPassword = IRCPassword,
            ServerURL = ServerURL
        }));
    }
}
