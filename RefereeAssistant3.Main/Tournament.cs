using Newtonsoft.Json;
using RefereeAssistant3.Main.Storage;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Tournament
    {
        public string TournamentName { get; }

        public bool DoFailedScoresCount { get; }

        public IEnumerable<TournamentStage> Stages { get; } = new List<TournamentStage>();

        public IEnumerable<TeamStorage> Teams { get; } = new List<TeamStorage>();

        public Tournament(TournamentConfiguration configuration, IEnumerable<TournamentStage> stages, IEnumerable<TeamStorage> teams)
        {
            TournamentName = configuration.Name;
            Stages = stages;
            Teams = teams;
        }

        public override string ToString() => TournamentName;

        public void Save()
        {
            var path = $"{Utilities.GetBaseDirectory()}/tournaments/{TournamentName}";
            foreach (var character in Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()))
                path.Replace(character.ToString(), "");
            var dir = new DirectoryInfo(path);
            dir.Create();

            File.WriteAllText($"{dir}/configuration.json", JsonConvert.SerializeObject(new TournamentConfiguration
            {
                DoFailedScoresCount = DoFailedScoresCount,
                Name = TournamentName
            }));
            File.WriteAllText($"{dir}/stages.json", JsonConvert.SerializeObject(Stages));
            File.WriteAllText($"{dir}/teams.json", JsonConvert.SerializeObject(Teams));
        }
    }
}
