using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class MatchSnapshot
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

        [JsonIgnore]
        [BsonIgnore]
        public IReadOnlyList<Map> T1PickedMaps { get; }
        [JsonIgnore]
        [BsonIgnore]
        public IReadOnlyList<Map> T2PickedMaps { get; }
        [JsonIgnore]
        [BsonIgnore]
        public IReadOnlyList<Map> T1BannedMaps { get; }
        [JsonIgnore]
        [BsonIgnore]
        public IReadOnlyList<Map> T2BannedMaps { get; }
        [JsonIgnore]
        [BsonIgnore]
        public Map CurrentMap { get; }
        [JsonIgnore]
        [BsonIgnore]
        public Team CurrentWinner { get; }

        public DateTime Time { get; set; }

        public MatchSnapshot() { }

        public MatchSnapshot(Match match, string name)
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
            T1PickedMaps = match.Team1.PickedMaps.ToList();
            T2PickedMaps = match.Team2.PickedMaps.ToList();
            T1BannedMaps = match.Team1.BannedMaps.ToList();
            T2BannedMaps = match.Team2.BannedMaps.ToList();
            Team1PickedMaps = T1PickedMaps.Select(m => m.DifficultyId.Value).ToList();
            Team2PickedMaps = T2PickedMaps.Select(m => m.DifficultyId.Value).ToList();
            Team1BannedMaps = T1BannedMaps.Select(m => m.DifficultyId.Value).ToList();
            Team2BannedMaps = T2BannedMaps.Select(m => m.DifficultyId.Value).ToList();
            CurrentMap = match.SelectedMap;
            SelectedMap = CurrentMap?.DifficultyId;
            CurrentWinner = match.SelectedWinner;
            SelectedWinner = CurrentWinner?.TeamName;
        }
    }
}
