using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public List<MatchProcedure> Procedures = new List<MatchProcedure>();

        public MatchProcedure CurrentProcedure => Procedures[currentProcedureIndex];
        private int currentProcedureIndex;

        public Team RollWinner;
        public Team RollLoser => Team1 == RollWinner ? Team2 : Team1;

        public Map SelectedMap;

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
            { MatchProcedure.Playing, $"Playing {SelectedMap?.MapCode}: {SelectedMap?.DisplayName}" },
            { MatchProcedure.FreePoint1, $"{Team1} receives a free point" },
            { MatchProcedure.FreePoint2, $"{Team2} receives a free point" }
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

            Procedures.Add(MatchProcedure.SettingUp);
            foreach (var procedureParameter in tournamentStage.MatchProceedings)
                GenerateMatchProcedure(procedureParameter.ToLowerInvariant());
        }

        public string ReadableCurrentState => readableMatchStateDictionary[CurrentProcedure];

        private void GenerateMatchProcedure(string param)
        {
            if (warmup_regex.IsMatch(param))
                GenerateWarmupProcedure(TeamFromLetter(warmup_regex.Match(param).Groups[1].Value));
            else if (roll_regex.IsMatch(param))
                GenerateRollingProcedure();
            else if (pick_regex.IsMatch(param))
                GeneratePickingProcedure(TeamFromLetter(pick_regex.Match(param).Groups[1].Value));
            else if (ban_regex.IsMatch(param))
                GenerateBanningProcedure(TeamFromLetter(ban_regex.Match(param).Groups[1].Value));
            else if (tiebreaker_regex.IsMatch(param))
                GenerateTiebreakerProcedure();
            else if (free_point_regex.IsMatch(param))
                GenerateFreePointProcedure(TeamFromLetter(free_point_regex.Match(param).Groups[1].Value));
        }

        private void GenerateWarmupProcedure(ProcedureTeam team)
        {
            Procedures.Add(IsProcedureTeamTeam1(team) ? MatchProcedure.WarmUp1 : MatchProcedure.WarmUp2);
            Procedures.Add(MatchProcedure.GettingReady);
            Procedures.Add(MatchProcedure.Playing);
        }

        private void GenerateRollingProcedure() => Procedures.Add(MatchProcedure.Rolling);

        private void GeneratePickingProcedure(ProcedureTeam team)
        {
            Procedures.Add(IsProcedureTeamTeam1(team) ? MatchProcedure.Picking1 : MatchProcedure.Picking2);
            Procedures.Add(MatchProcedure.GettingReady);
            Procedures.Add(MatchProcedure.Playing);
        }

        private void GenerateBanningProcedure(ProcedureTeam team) => Procedures.Add(IsProcedureTeamTeam1(team) ? MatchProcedure.Banning1 : MatchProcedure.Banning2);

        private void GenerateTiebreakerProcedure()
        {
            Procedures.Add(MatchProcedure.TieBreaker);
            Procedures.Add(MatchProcedure.GettingReady);
            Procedures.Add(MatchProcedure.Playing);
        }

        private void GenerateFreePointProcedure(ProcedureTeam team) => Procedures.Add(IsProcedureTeamTeam1(team) ? MatchProcedure.FreePoint1 : MatchProcedure.FreePoint2);

        private static readonly Regex warmup_regex = new Regex(@"^warm(1|2|w|l)$");
        private static readonly Regex roll_regex = new Regex(@"^roll$");
        private static readonly Regex pick_regex = new Regex(@"^p(1|2|w|l)$");
        private static readonly Regex ban_regex = new Regex(@"^b(1|2|w|l)$");
        private static readonly Regex tiebreaker_regex = new Regex(@"^tb$");
        private static readonly Regex free_point_regex = new Regex(@"^free(1|2|w|l)$");

        private ProcedureTeam TeamFromLetter(string letter)
        {
            switch (letter)
            {
                case "w":
                    return ProcedureTeam.RollWinner;
                case "l":
                    return ProcedureTeam.RollLoser;
                case "1":
                    return ProcedureTeam.Team1;
                case "2":
                    return ProcedureTeam.Team2;
                default:
                    throw new Exception();
            }
        }

        private bool IsProcedureTeamTeam1(ProcedureTeam procedureTeam)
        {
            if (procedureTeam == ProcedureTeam.Team1)
                return true;
            if (procedureTeam == ProcedureTeam.RollWinner && RollWinner == Team1)
                return true;
            else
                return false;
        }

        public bool Proceed()
        {
            if (CurrentProcedure == MatchProcedure.SettingUp)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.WarmUp1 || CurrentProcedure == MatchProcedure.WarmUp2)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.Rolling && RollWinner != null)
                return GoToNextProcedure();
            if (SelectedMap != null)
            {
                if (CurrentProcedure == MatchProcedure.Banning1)
                {
                    Team1.BannedMaps.Add(SelectedMap);
                }
                if (CurrentProcedure == MatchProcedure.Banning2)
                {
                    Team2.BannedMaps.Add(SelectedMap);
                }
                if (CurrentProcedure == MatchProcedure.Picking1)
                {
                    Team1.PickedMaps.Add(SelectedMap);
                }
                if (CurrentProcedure == MatchProcedure.Picking2)
                {
                    Team2.PickedMaps.Add(SelectedMap);
                }
            }
            if (CurrentProcedure == MatchProcedure.GettingReady)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.TieBreaker)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.Playing)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.FreePoint1 || CurrentProcedure == MatchProcedure.FreePoint2)
                return GoToNextProcedure();
            return false;
        }

        private bool GoToNextProcedure()
        {
            currentProcedureIndex++;
            DoNewProcedureJobs();
            Updated?.Invoke();
            return true;
        }

        private void DoNewProcedureJobs()
        {
            if (CurrentProcedure == MatchProcedure.Rolling)
                RollWinner = null;
            if (CurrentProcedure == MatchProcedure.Banning1 || CurrentProcedure == MatchProcedure.Banning2)
                SelectedMap = null;
        }
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
        FreePoint1 = 11,
        FreePoint2 = 12,
    }

    public enum ProcedureTeam
    {
        Team1 = 0,
        Team2 = 1,
        RollWinner = 2,
        RollLoser = 3
    }
}
