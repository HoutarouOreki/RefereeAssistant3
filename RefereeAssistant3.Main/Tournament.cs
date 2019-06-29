using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Tournament
    {
        public string TournamentName { get; }

        public bool DoFailedScoresCount { get; }

        public IEnumerable<TournamentStage> Stages { get; } = new List<TournamentStage>();

        public IEnumerable<Team> Teams { get; } = new List<Team>();

        public Tournament(string configuration, string stagesFile, string teamsFile)
        {
            configuration = configuration.Replace("\r", "").Trim();
            stagesFile = stagesFile.Replace("\r", "").Trim();
            teamsFile = teamsFile.Replace("\r", "").Trim();

            TournamentName = configuration.Split('\n')[0];
            DoFailedScoresCount = configuration.Split('\n')[1] == "yes";

            var stageTexts = stagesFile.Split("\n###\n");
            var stages = new List<TournamentStage>();
            foreach (var stageText in stageTexts)
                stages.Add(new TournamentStage(stageText));
            Stages = stages;

            var teams = new List<Team>();
            foreach (var teamText in teamsFile.Split('\n'))
            {
                var membersText = teamText.Split(':')[1].Split(',');
                teams.Add(new Team(teamText.Split(':')[0], membersText.Select(m => new Player(m))));
            }
            Teams = teams;
        }

        public Tournament(TournamentConfiguration configuration, IEnumerable<TournamentStage> stages, IEnumerable<Team> teams)
        {
            TournamentName = configuration.Name;
            Stages = stages;
            Teams = teams;
        }

        public override string ToString() => TournamentName;
    }
}
