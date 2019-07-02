using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using RefereeAssistant3.IRC;
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

        public string RollWinnerTeamName;

        public IrcChannel Chat;

        [BsonIgnore]
        [JsonIgnore]
        public APITeam RollWinner => RollWinnerTeamName == Team1?.TeamName ? Team1
            : RollWinnerTeamName == Team2?.TeamName ? Team2
            : null;

        public List<MatchSnapshot> History;
    }
}
