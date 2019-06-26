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
        public MatchProcedure CurrentProcedure = MatchProcedure.SettingUp;

        public Team RollWinner;
        public Team RollLoser => Team1 == RollWinner ? Team2 : Team1;

        public Map CurrentMap;

        public Dictionary<Team, int> Score = new Dictionary<Team, int>();

        public readonly Dictionary<Map, Dictionary<Player, int>> MapResults = new Dictionary<Map, Dictionary<Player, int>>();

        // after warmups, picks and tiebreaker a map is played
        private Dictionary<MatchProcedure, string> readableMatchStateDictionary => new Dictionary<MatchProcedure, string>
        {
            { MatchProcedure.SettingUp, "Setting up the match" },
            { MatchProcedure.WarmUp1, $"Warmup by {Team1}" },
            { MatchProcedure.WarmUp2, $"Warmup by {Team2}" },
            { MatchProcedure.Rolling, $"Teams are rolling" },
            { MatchProcedure.Banning1, $"{Team1} are banning" },
            { MatchProcedure.Banning2, $"{Team2} are banning" },
            { MatchProcedure.Picking1, $"{Team1} are picking" },
            { MatchProcedure.Picking2, $"{Team2} are picking" },
            { MatchProcedure.GettingReady, "Players are getting ready" },
            { MatchProcedure.TieBreaker, "Tiebreaker!" },
            { MatchProcedure.Playing, $"Playing {CurrentMap?.MapCode}: {CurrentMap?.DisplayName}" }
        };

        public string Title => TournamentStage.RoomName.Replace("TEAM1", Team1.TeamName).Replace("TEAM2", Team2.TeamName);

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

        public void BeginRollingPhase() => CurrentProcedure = MatchProcedure.Rolling;

        /// <summary>
        /// Automatically begins banning phase.
        /// </summary>
        public void SpecifyRollWinner(Team team)
        {
            RollWinner = team;
        }

        public string ReadableCurrentState => readableMatchStateDictionary[CurrentProcedure];
    }

    public enum MatchProcedure
    {
        SettingUp = 0,
        WarmUp1 = 1,
        WarmUp2 = 2,
        Rolling = 3,
        Banning1 = 4,
        Banning2 = 5,
        Picking1 = 6,
        Picking2 = 7,
        GettingReady = 8,
        TieBreaker = 9,
        Playing = 10,
    }
}
