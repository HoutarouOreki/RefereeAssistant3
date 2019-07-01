﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Team
    {
        [JsonRequired]
        public readonly string TeamName;

        [JsonRequired]
        public readonly List<Player> Members;

        [JsonIgnore]
        public List<Map> BannedMaps = new List<Map>();

        [JsonIgnore]
        public List<Map> PickedMaps = new List<Map>();

        public Team(string teamName, IEnumerable<Player> members)
        {
            TeamName = teamName;
            Members = members.ToList();
        }

        public Team() { }

        public override string ToString() => TeamName;
    }
}
