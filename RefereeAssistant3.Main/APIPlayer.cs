using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace RefereeAssistant3.Main
{
    public class APIPlayer
    {
        [BsonRequired]
        [JsonRequired]
        public int PlayerId;

        public APIPlayer(int playerId) => PlayerId = playerId;
    }
}
