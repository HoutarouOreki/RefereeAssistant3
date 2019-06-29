using Newtonsoft.Json;

namespace RefereeAssistant3.Main
{
    public class TournamentConfiguration
    {
        [JsonRequired]
        public string Name;

        public bool DoFailedScoresCount;
    }
}
