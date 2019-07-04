using Newtonsoft.Json;
using osuTK;
using RefereeAssistant3.Main;
using System.IO;

namespace RefereeAssistant3.Visual
{
    public static class VisualConfig
    {
        public static WindowState WindowState = WindowState.Normal;

        private readonly static string config_path = $"{Utilities.RootProgramDirectory}/visualConfig.json";

        public static void Load()
        {
            VisualConfigFile file;
            if (!File.Exists(config_path))
            {
                file = new VisualConfigFile();
                File.WriteAllText(config_path, JsonConvert.SerializeObject(file));
            }
            else
                file = JsonConvert.DeserializeObject<VisualConfigFile>(File.ReadAllText(config_path));

            WindowState = file.WindowState;
        }

        public static void Save() => File.WriteAllTextAsync(config_path, JsonConvert.SerializeObject(new VisualConfigFile
        {
            WindowState = WindowState
        }));
    }
}
