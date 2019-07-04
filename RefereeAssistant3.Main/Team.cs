using RefereeAssistant3.Main.Storage;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Team
    {
        public readonly string TeamName;

        public readonly HashSet<Player> Members;

        public List<Map> BannedMaps = new List<Map>();

        public List<Map> PickedMaps = new List<Map>();

        public Team(string teamName, IEnumerable<Player> members)
        {
            TeamName = teamName;
            Members = members.ToHashSet();
        }

        public Team(TeamStorage team)
        {
            TeamName = team.TeamName;
            Members = team.Members.Select(apiPlayer => new Player(apiPlayer.PlayerId)).ToHashSet();
        }

        public Team() { }

        public Team(APITeam apiTeam)
        {
            TeamName = apiTeam.TeamName;
            Members = apiTeam.Members.Select(apiPlayer => new Player(apiPlayer.PlayerId)).ToHashSet();
            BannedMaps = apiTeam.BannedMaps.Select(m => new Map(m)).ToList();
            PickedMaps = apiTeam.PickedMaps.Select(m => new Map(m)).ToList();
        }

        public override string ToString() => TeamName;

        public override bool Equals(object obj)
        {
            if (!(obj is Team team))
                return false;
            if (TeamName != team.TeamName)
                return false;
            if (!Members.All(m => team.Members.Any(mm => mm.Equals(m))))
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hash = TeamName.GetHashCode();
            hash -= Members.GetHashCode();
            return hash;
        }
    }
}
