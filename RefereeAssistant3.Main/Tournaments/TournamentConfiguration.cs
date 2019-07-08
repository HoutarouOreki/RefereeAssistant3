using Newtonsoft.Json;

namespace RefereeAssistant3.Main.Tournaments
{
    public class TournamentConfiguration
    {
        [JsonRequired]
        public string TournamentName;

        public override string ToString() => TournamentName;
    }
}
