using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public struct MatchSnapshot
    {
        public IReadOnlyDictionary<Map, IReadOnlyDictionary<Player, int>> MapResults { get; }
        public string Name { get; }
        public int ProcedureIndex { get; }
        public Team RollWinner { get; }
        public IReadOnlyDictionary<Team, int> Scores { get; }
        public IReadOnlyList<Map> Team1PickedMaps { get; }
        public IReadOnlyList<Map> Team2PickedMaps { get; }
        public IReadOnlyList<Map> Team1BannedMaps { get; }
        public IReadOnlyList<Map> Team2BannedMaps { get; }
        public DateTime Time { get; }

        public MatchSnapshot(Match match, string name)
        {
            MapResults = match.MapResults;
            Name = name;
            ProcedureIndex = match.CurrentProcedureIndex;
            Scores = match.Scores;
            RollWinner = match.RollWinner;
            Time = DateTime.UtcNow;
            Team1PickedMaps = match.Team1.PickedMaps;
            Team2PickedMaps = match.Team2.PickedMaps;
            Team1BannedMaps = match.Team1.BannedMaps;
            Team2BannedMaps = match.Team2.BannedMaps;
        }
    }
}
