using MongoDB.Bson.Serialization.Attributes;
using RefereeAssistant3.Main.IRC;
using RefereeAssistant3.Main.Matches;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.APIModels
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

        public List<TeamVsMatchSnapshot> History;

        public string TournamentName;
        public string TournamentStage;
    }
}
