using System.Collections.Generic;

namespace RefereeAssistant3.Main.Matches
{
    public abstract class MatchParticipant
    {
        public abstract string Name { get; }

        public List<Map> BannedMaps { get; set; } = new List<Map>();

        public List<Map> PickedMaps { get; set; } = new List<Map>();
    }
}
