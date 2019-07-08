using MongoDB.Bson.Serialization.Attributes;
using RefereeAssistant3.Main.Tournaments;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.Online.APIModels
{
    public class APITeam : APIParticipant
    {
        [BsonRequired]
        public List<APIPlayer> Members;

        public APITeam(Team team, int score) : base(team, score) => Members = team.Members.Select(m => new APIPlayer(m.PlayerId.Value)).ToList();
    }
}
