using System.Collections.Generic;

namespace RefereeAssistant3.Main.Matches
{
    public interface IMatchParticipant
    {
        string Name { get; }

        List<Map> BannedMaps { get; }

        List<Map> PickedMaps { get; }
    }
}
