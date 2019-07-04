using Newtonsoft.Json;
using RefereeAssistant3.Main.APIModels;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.Storage
{
    public class TeamStorage
    {
        [JsonRequired]
        public string TeamName;

        [JsonRequired]
        public List<APIPlayer> Members;

        public TeamStorage(string teamName, List<APIPlayer> members)
        {
            TeamName = teamName;
            Members = members;
        }
    }
}
