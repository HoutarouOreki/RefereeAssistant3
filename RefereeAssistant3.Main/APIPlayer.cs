using MongoDB.Bson.Serialization.Attributes;

namespace RefereeAssistant3.Main
{
    public class APIPlayer
    {
        [BsonRequired]
        public int PlayerId;
    }
}
