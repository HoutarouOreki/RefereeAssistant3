using Newtonsoft.Json;
using RefereeAssistant3.Main.Storage;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Tournament
    {
        public TournamentConfiguration Configuration { get; }
        public List<TournamentStage> Stages { get; } = new List<TournamentStage>();
        public List<TeamStorage> Teams { get; } = new List<TeamStorage>();

        public Tournament(TournamentConfiguration configuration, IEnumerable<TournamentStage> stages, IEnumerable<TeamStorage> teams)
        {
            Configuration = configuration;
            Stages = stages.ToList();
            Teams = teams.ToList();
        }

        public override string ToString() => Configuration.TournamentName;

        public void Save()
        {
            var dir = new DirectoryInfo(GetPathFromName(Configuration.TournamentName));
            dir.Create();

            File.WriteAllText($"{dir}/configuration.json", JsonConvert.SerializeObject(new TournamentConfiguration
            {
                DoFailedScoresCount = Configuration.DoFailedScoresCount,
                TournamentName = Configuration.TournamentName
            }));
            File.WriteAllText($"{dir}/stages.json", JsonConvert.SerializeObject(Stages));
            File.WriteAllText($"{dir}/teams.json", JsonConvert.SerializeObject(Teams));
        }

        public static string GetPathFromName(string name)
        {
            var path = $"{Utilities.GetBaseDirectory()}/tournaments/{name}";
            foreach (var character in Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()))
                path.Replace(character.ToString(), "");
            path = path.ToLowerInvariant();
            return path;
        }
    }
}
