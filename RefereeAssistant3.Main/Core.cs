using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace RefereeAssistant3.Main
{
    public class Core
    {
        private static readonly string config_path = $"{Utilities.GetBaseDirectory()}/mainConfig.json";
        public event Action<Match> NewMatchAdded;

        public MainConfig Config;

        public Match SelectedMatch { get; set; }

        public IReadOnlyList<Match> Matches => matches;
        public IEnumerable<Tournament> Tournaments { get; }

        private readonly List<Match> matches = new List<Match>();

        public Core(IEnumerable<Tournament> tournaments)
        {
            Tournaments = tournaments;
            if (!File.Exists(config_path))
            {
                Config = new MainConfig();
                SaveConfig();
            }
            else
                Config = JsonConvert.DeserializeObject<MainConfig>(File.ReadAllText(config_path));
        }

        public void SaveConfig() => File.WriteAllText(config_path, JsonConvert.SerializeObject(Config));

        public void AddNewMatch(Match match)
        {
            match.TournamentStage.Mappool.DownloadMappoolAsync(this);
            matches.Add(match);
            NewMatchAdded?.Invoke(match);
        }
    }
}
