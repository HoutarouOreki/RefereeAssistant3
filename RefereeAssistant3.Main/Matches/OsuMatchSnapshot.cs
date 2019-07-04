using RefereeAssistant3.Main.Online.APIModels;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.Matches
{
    public class OsuMatchSnapshot
    {
        public DateTime Time { get; } = DateTime.UtcNow;
        public IReadOnlyList<APIParticipant> Participants { get; set; }
        public IReadOnlyList<MapResult> MapResults { get; set; }
        public string Name { get; set; }
        public int ProcedureIndex { get; set; }
        public string RollWinnersName { get; set; }
        public int? SelectedMap { get; set; }
        public APIParticipant SelectedWinner { get; set; }
    }
}
