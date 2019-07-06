using MongoDB.Bson.Serialization.Attributes;
using RefereeAssistant3.Main.IRC;
using RefereeAssistant3.Main.Matches;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.Online.APIModels
{
    public class APIMatch
    {
        [BsonRequired]
        public int Id;
        public string Code;

        public List<APIParticipant> Participants;

        public List<APIPlayer> Players;

        public MpRoomIrcChannel Chat;

        public List<OsuMatchSnapshot> History;

        public string TournamentName;
        public string TournamentStage;
    }
}
