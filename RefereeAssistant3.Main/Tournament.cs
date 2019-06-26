using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Tournament
    {
        public string TournamentName { get; }

        public bool DoFailedScoresCount { get; }

        public List<TournamentStage> Stages { get; } = new List<TournamentStage>();

        public List<Team> Teams { get; } = new List<Team>();

        public Tournament(string configuration, string stagesFile, string teamsFile)
        {
            configuration = configuration.Replace("\r", "").Trim();
            stagesFile = stagesFile.Replace("\r", "").Trim();
            teamsFile = teamsFile.Replace("\r", "").Trim();

            TournamentName = configuration.Split('\n')[0];
            DoFailedScoresCount = configuration.Split('\n')[1] == "yes";

            var stageTexts = stagesFile.Split("\n###\n");
            foreach (var stageText in stageTexts)
                Stages.Add(new TournamentStage(stageText));

            foreach (var teamText in teamsFile.Split('\n'))
            {
                var membersText = teamText.Split(':')[1].Split(',');
                Teams.Add(new Team(teamText.Split(':')[0], membersText.Select(m => new Player(m))));
            }
        }

        public override string ToString() => TournamentName;
    }
}
