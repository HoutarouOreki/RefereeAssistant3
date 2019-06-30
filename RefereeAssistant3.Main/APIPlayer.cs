using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class APIPlayer
    {
        [BsonRequired]
        public int PlayerId;
    }
}
