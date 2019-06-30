using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class APITeam
    {
        [BsonRequired]
        public string TeamName;

        [BsonRequired]
        public List<APIPlayer> Members;

        public int Score;
        public List<int> PickedMaps;
        public List<int> BannedMaps;
    }
}
