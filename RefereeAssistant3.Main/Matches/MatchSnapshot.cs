using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.Matches
{
    public class TeamVsMatchSnapshot
    {
        public IReadOnlyList<MapResult> MapResults { get; set; }
        public string Name { get; set; }
        public int ProcedureIndex { get; set; }
        public string RollWinnerTeamName { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }

        public IReadOnlyList<int> Team1PickedMaps { get; set; }
        public IReadOnlyList<int> Team2PickedMaps { get; set; }
        public IReadOnlyList<int> Team1BannedMaps { get; set; }
        public IReadOnlyList<int> Team2BannedMaps { get; set; }
        public int? SelectedMap { get; set; }
        public string SelectedWinner { get; set; }

        public DateTime Time { get; set; }

        public TeamVsMatchSnapshot() { }

        public TeamVsMatchSnapshot(TeamVsMatch match, string name)
        {
            var mapResults = new List<MapResult>();
            foreach (var map in match.MapResults)
            {
                var mapResult = new MapResult
                {
                    DifficultyId = map.Key.DifficultyId.Value,
                    PlayerScores = new Dictionary<int, int>()
                };
                foreach (var playerScore in map.Value)
                    mapResult.PlayerScores.Add(playerScore.Key.PlayerId.Value, playerScore.Value);
            }
            Name = name;
            ProcedureIndex = match.CurrentProcedureIndex;
            MapResults = mapResults;
            RollWinnerTeamName = match.RollWinner?.TeamName;
            Time = DateTime.UtcNow;
            Team1Score = match.Scores[match.Team1];
            Team2Score = match.Scores[match.Team2];
            Team1PickedMaps = match.Team1.PickedMaps.Select(m => m.DifficultyId.Value).ToList();
            Team2PickedMaps = match.Team2.PickedMaps.Select(m => m.DifficultyId.Value).ToList();
            Team1BannedMaps = match.Team1.BannedMaps.Select(m => m.DifficultyId.Value).ToList();
            Team2BannedMaps = match.Team2.BannedMaps.Select(m => m.DifficultyId.Value).ToList();
            SelectedMap = match.SelectedMap?.DifficultyId;
            SelectedWinner = match.SelectedWinner?.TeamName;
        }
    }
}
