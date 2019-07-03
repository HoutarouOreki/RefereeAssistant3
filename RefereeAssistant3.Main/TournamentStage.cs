using Newtonsoft.Json;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class TournamentStage
    {
        [JsonRequired]
        public string TournamentStageName;

        public string RoomName;

        [JsonRequired]
        public List<string> MatchProceedings;

        [JsonRequired]
        public int ScoreRequiredToWin;

        public Mappool Mappool = new Mappool();

        public TournamentStage() { }

        public TournamentStage(string tournamentStageName, string roomName, List<string> matchProceedings, int scoreRequiredToWin, Mappool mappool)
        {
            TournamentStageName = tournamentStageName;
            RoomName = roomName;
            MatchProceedings = matchProceedings;
            ScoreRequiredToWin = scoreRequiredToWin;
            Mappool = mappool;
        }

        public override string ToString() => TournamentStageName;
    }
}
