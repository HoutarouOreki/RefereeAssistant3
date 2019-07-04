using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class APIMatch
    {
        [BsonRequired]
        public int Id;
        public string Code;

        public APITeam Team1;
        public APITeam Team2;

        public List<MapResult> MapResults;

        public MpRoomIrcChannel Chat;

        public List<MatchSnapshot> History;

        public string TournamentName;
        public string TournamentStage;
    }
}
