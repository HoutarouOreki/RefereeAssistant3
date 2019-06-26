using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class Match
    {
        public readonly Team Team1;
        public readonly Team Team2;

        public readonly List<IOperation> History = new List<IOperation>(); // in case we need to undo anything

        public event Action Updated;

        public readonly Mappool Mappool;

        public readonly Tournament Tournament;
        public readonly TournamentStage TournamentStage;
        public MatchState State = MatchState.SettingUp;

        public Team RollWinner;

        public Map CurrentMap;

        public Dictionary<Team, int> Score = new Dictionary<Team, int>();

        public readonly Dictionary<Map, Dictionary<Player, int>> MapResults = new Dictionary<Map, Dictionary<Player, int>>();

        public string Title => ComputeTitle();

        public Match(Team team1, Team team2, Tournament tournament, TournamentStage tournamentStage)
        {
            // store info about the match
            Team1 = team1;
            Team2 = team2;
            Score.Add(Team1, 0);
            Score.Add(Team2, 0);
            Tournament = tournament;
            TournamentStage = tournamentStage;
        }

        private string ComputeTitle()
        {
            var prefix = $"{TournamentStage}";
            return $"{prefix}: {Team1.TeamName} vs {Team2.TeamName}";
        }

        public void BeginRollingPhase() => State = MatchState.Rolling;

        /// <summary>
        /// Automatically begins banning phase.
        /// </summary>
        public void SpecifyRollWinner(Team team)
        {
            RollWinner = team;
            BeginBanningPhase();
        }

        public void BeginBanningPhase() => State = MatchState.Banning;

        public void BanMap(Map map)
        {
        }

        public void BeginPickingPhase() => State = MatchState.Picking;

        public void PickMap(Map map)
        {
        }

        public void BeginWaitingForPlayersToGetReady() => State = MatchState.GettingReady;

        public void StartMap() => State = MatchState.Playing;

        public void AddResults(Dictionary<Player, int> scores) => MapResults.Add(CurrentMap, scores);
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
