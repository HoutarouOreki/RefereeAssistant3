using Newtonsoft.Json;

namespace RefereeAssistant3.Main.Tournaments
{
    public class TournamentConfiguration
    {
        [JsonRequired]
        public string TournamentName;

        public bool DoFailedScoresCount;

        public TournamentConfiguration(string tournamentName, bool doFailedScoresCount = false)
        {
            TournamentName = tournamentName;
            DoFailedScoresCount = doFailedScoresCount;
        }

        public TournamentConfiguration() { }
    }
}
