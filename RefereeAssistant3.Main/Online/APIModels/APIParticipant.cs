using MongoDB.Bson.Serialization.Attributes;
using RefereeAssistant3.Main.Matches;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.Online.APIModels
{
    public class APIParticipant
    {
        [BsonRequired]
        public string Name;

        public int? Score;
        public List<int> BannedMaps;
        public List<int> PickedMaps;

        public APIParticipant() { }

        public APIParticipant(MatchParticipant participant, int? score = null)
        {
            Name = participant.Name;
            Score = score;
            BannedMaps = participant.BannedMaps.Select(m => m.DifficultyId).ToList();
            PickedMaps = participant.PickedMaps.Select(m => m.DifficultyId).ToList();
        }
    }
}
