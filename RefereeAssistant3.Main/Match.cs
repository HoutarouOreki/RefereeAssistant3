using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Match
    {
        public readonly List<Player> Players = new List<Player>();
        public readonly List<IOperation> History = new List<IOperation>(); // in case we need to undo anything
        public readonly Mappool Mappool;
        public readonly Dictionary<Player, List<Map>> BannedMaps = new Dictionary<Player, List<Map>>();
        public readonly Dictionary<Player, List<Map>> PickedMaps = new Dictionary<Player, List<Map>>();
        public readonly TournamentStage TournamentStage;
        public MatchState State;
        public Player RollWinner;
        public Player CurrentlyBanningPlayer => Players[currentlyBanningIndex];
        public Player CurrentlyPickingPlayer => Players[currentlyPickingIndex];
        public Map CurrentMap;
        public int AmountOfBansPerRound = 2;
        public int AmountOfPicksPerRound = 4;
        public readonly Dictionary<Map, Dictionary<Player, int>> MapResults = new Dictionary<Map, Dictionary<Player, int>>();

        public readonly Dictionary<Player, int> ManualScoreOffsets = new Dictionary<Player, int>();

        public Dictionary<Player, int> WonMaps
        {
            get
            {
                var dictionary = new Dictionary<Player, int>();
                foreach (var player in Players)
                    dictionary.Add(player, 0);
                foreach (var map in MapResults)
                {
                    Player winner = null;
                    var highestScore = int.MinValue;
                    // warning: doesn't account for draws
                    foreach (var playerScore in map.Value)
                    {
                        if (playerScore.Value > highestScore)
                            winner = playerScore.Key;
                    }
                    dictionary[winner]++;
                }
                return dictionary;
            }
        }

        private int currentlyPickingIndex
        {
            get
            {
                var index = Players.IndexOf(RollWinner) + banningOrPickingIndex;
                if (index >= Players.Count)
                    index -= Players.Count;
                return index;
            }
        }
        private int currentlyBanningIndex
        {
            get
            {
                var index = Players.IndexOf(personToFirstBan) + banningOrPickingIndex;
                if (index >= Players.Count)
                    index -= Players.Count;
                return index;
            }
        }
        private int banningOrPickingIndex;
        private Player personToFirstBan
        {
            get
            {
                var index = Players.IndexOf(RollWinner) + 1;
                if (index >= Players.Count)
                    index -= Players.Count;
                return Players[index];
            }
        }

        public string Title => ComputeTitle();

        public Match(Player[] players, Mappool mappool, TournamentStage tournamentStage)
        {
            // store info about the match
            Players.AddRange(players);
            Mappool = mappool;
            TournamentStage = tournamentStage;

            // initialize map bans and picks for each player
            foreach (var player in Players)
            {
                BannedMaps.Add(player, new List<Map>());
                PickedMaps.Add(player, new List<Map>());
            }
        }

        private string ComputeTitle()
        {
            var prefix = $"{TournamentStage}";
            if (Players.Count == 2)
                return $"{prefix}: {Players[0].Username} vs {Players[1].Username}";
            else
                return $"{prefix}: {string.Join(", ", Players.Select(p => p.Username))}";
        }

        public void BeginRollingPhase() => State = MatchState.Rolling;

        /// <summary>
        /// Automatically begins banning phase.
        /// </summary>
        public void SpecifyRollWinner(Player player)
        {
            RollWinner = player;
            BeginBanningPhase();
        }

        public void BeginBanningPhase() => State = MatchState.Banning;

        public void BanMap(Map map)
        {
            BannedMaps[CurrentlyBanningPlayer].Add(map);
            banningOrPickingIndex++;
            if (banningOrPickingIndex + 1 == AmountOfBansPerRound * 2)
            {
                banningOrPickingIndex = 0;
                BeginPickingPhase();
            }
        }

        public void BeginPickingPhase() => State = MatchState.Picking;

        public void PickMap(Map map)
        {
            PickedMaps[CurrentlyPickingPlayer].Add(map);
            CurrentMap = map;
            banningOrPickingIndex++;
            if (banningOrPickingIndex + 1 == AmountOfPicksPerRound * 2)
            {
                banningOrPickingIndex = 0;
                BeginWaitingForPlayersToGetReady();
            }
        }

        public void BeginWaitingForPlayersToGetReady() => State = MatchState.GettingReady;

        public void StartMap() => State = MatchState.Playing;

        public void AddResults(Dictionary<Player, int> scores) => MapResults.Add(CurrentMap, scores);
    }

    public enum TournamentStage
    {
        Unspecified = 0,
        Qualifiers = 1,
    }

    public enum MatchState
    {
        SettingUp = 0,
        Rolling = 1,
        Banning = 2,
        Picking = 3,
        GettingReady = 4,
        Playing = 5,
    }
}
