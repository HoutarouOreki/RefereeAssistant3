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

        public Tournament(TournamentConfiguration configuration, IEnumerable<TournamentStage> stages, IEnumerable<Team> teams)
        {
            TournamentName = configuration.Name;
            Stages = stages;
            Teams = teams;
        }

        public override string ToString() => TournamentName;
    }
}
