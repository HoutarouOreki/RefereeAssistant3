﻿using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class Match
    {
        public readonly Team Team1;
        public readonly Team Team2;

        private readonly List<MatchSnapshot> history = new List<MatchSnapshot>();
        public IReadOnlyList<MatchSnapshot> History => history;
        private readonly List<MatchSnapshot> cancelledOperations = new List<MatchSnapshot>();
        public IReadOnlyList<MatchSnapshot> CancelledOperations => cancelledOperations;

        public event Action Updated;
        public event Action<Match, string> Alert;

        private readonly List<Map> playedMaps = new List<Map>();
        public IReadOnlyList<Map> PlayedMaps => playedMaps;

        public readonly Tournament Tournament;
        public readonly TournamentStage TournamentStage;

        public bool IsFinished => Scores[Team1] == TournamentStage.ScoreRequiredToWin || Scores[Team2] == TournamentStage.ScoreRequiredToWin;

        public List<MatchProcedure> Procedures = new List<MatchProcedure>();
        private readonly ProcedureUtilities procedureUtilities;

        public MatchProcedure CurrentProcedure => Procedures[CurrentProcedureIndex];
        public int CurrentProcedureIndex { get; private set; }

        public Team RollWinner;
        public Team RollLoser => RollWinner == null ? null :
            Team1 == RollWinner ? Team2 : Team1;

        private Map selectedMap;
        public Map SelectedMap
        {
            get => selectedMap;
            set
            {
                selectedMap = value;
                Updated();
            }
        }

        public Dictionary<Team, int> Scores = new Dictionary<Team, int>();

        public readonly Dictionary<Map, IReadOnlyDictionary<Player, int>> MapResults = new Dictionary<Map, IReadOnlyDictionary<Player, int>>();

        public string Title => TournamentStage.RoomName.Replace("TEAM1", Team1.TeamName).Replace("TEAM2", Team2.TeamName);

        public Match(Team team1, Team team2, Tournament tournament, TournamentStage tournamentStage)
        {
            // store info about the match
            Team1 = team1;
            Team2 = team2;
            Scores.Add(Team1, 0);
            Scores.Add(Team2, 0);
            Tournament = tournament;
            TournamentStage = tournamentStage;

            procedureUtilities = new ProcedureUtilities(this);

            Procedures.Add(MatchProcedure.SettingUp);
            GenerateMatchProcedures();
        }

        private void GenerateMatchProcedures()
        {
            Procedures.Clear();
            Procedures.Add(MatchProcedure.SettingUp);
            foreach (var procedureParameter in TournamentStage.MatchProceedings)
                GenerateMatchProcedure(procedureParameter);
            foreach (var procedure in Procedures)
                Console.WriteLine($"{procedure}: {procedureUtilities.GetProcedureDescription(procedure)}");
        }

        public string ReadableCurrentState => procedureUtilities.GetProcedureDescription(CurrentProcedure);

        private void GenerateMatchProcedure(string param)
        {
            switch (param.ToLowerInvariant())
            {
                case "warm1":
                    GenerateWarmupProcedure(ProcedureTeam.Team1);
                    break;
                case "warm2":
                    GenerateWarmupProcedure(ProcedureTeam.Team2);
                    break;
                case "warmw":
                    GenerateWarmupProcedure(ProcedureTeam.RollWinner);
                    break;
                case "warml":
                    GenerateWarmupProcedure(ProcedureTeam.RollLoser);
                    break;
                case "roll":
                    GenerateRollingProcedure();
                    break;
                case "p1":
                    GeneratePickingProcedure(ProcedureTeam.Team1);
                    break;
                case "p2":
                    GeneratePickingProcedure(ProcedureTeam.Team2);
                    break;
                case "pw":
                    GeneratePickingProcedure(ProcedureTeam.RollWinner);
                    break;
                case "pl":
                    GeneratePickingProcedure(ProcedureTeam.RollLoser);
                    break;
                case "b1":
                    GenerateBanningProcedure(ProcedureTeam.Team1);
                    break;
                case "b2":
                    GenerateBanningProcedure(ProcedureTeam.Team2);
                    break;
                case "bw":
                    GenerateBanningProcedure(ProcedureTeam.RollWinner);
                    break;
                case "bl":
                    GenerateBanningProcedure(ProcedureTeam.RollLoser);
                    break;
                case "tb":
                    GenerateTiebreakerProcedure();
                    break;
                case "free1":
                    GenerateFreePointProcedure(ProcedureTeam.Team1);
                    break;
                case "free2":
                    GenerateFreePointProcedure(ProcedureTeam.Team2);
                    break;
                case "freew":
                    GenerateFreePointProcedure(ProcedureTeam.RollWinner);
                    break;
                case "freel":
                    GenerateFreePointProcedure(ProcedureTeam.RollLoser);
                    break;
            }
        }

        private void GenerateWarmupProcedure(ProcedureTeam team)
        {
            var procedure = team == ProcedureTeam.Team1 ? MatchProcedure.WarmUp1
                : team == ProcedureTeam.Team2 ? MatchProcedure.WarmUp2
                : team == ProcedureTeam.RollWinner ? MatchProcedure.WarmUpRollWinner
                : MatchProcedure.WarmUpRollLoser;

            Procedures.Add(procedure);
            Procedures.Add(MatchProcedure.GettingReady);
            Procedures.Add(MatchProcedure.Playing);
        }

        private void GenerateRollingProcedure() => Procedures.Add(MatchProcedure.Rolling);

        private void GeneratePickingProcedure(ProcedureTeam team)
        {
            var procedure = team == ProcedureTeam.Team1 ? MatchProcedure.Picking1
                : team == ProcedureTeam.Team2 ? MatchProcedure.Picking2
                : team == ProcedureTeam.RollWinner ? MatchProcedure.PickingRollWinner
                : MatchProcedure.PickingRollLoser;

            Procedures.Add(procedure);
            Procedures.Add(MatchProcedure.GettingReady);
            Procedures.Add(MatchProcedure.Playing);
        }

        private void GenerateBanningProcedure(ProcedureTeam team)
        {
            var procedure = team == ProcedureTeam.Team1 ? MatchProcedure.Banning1
                : team == ProcedureTeam.Team2 ? MatchProcedure.Banning2
                : team == ProcedureTeam.RollWinner ? MatchProcedure.BanningRollWinner
                : MatchProcedure.BanningRollLoser;

            Procedures.Add(procedure);
        }

        private void GenerateTiebreakerProcedure()
        {
            Procedures.Add(MatchProcedure.TieBreaker);
            Procedures.Add(MatchProcedure.GettingReady);
            Procedures.Add(MatchProcedure.Playing);
        }

        private void GenerateFreePointProcedure(ProcedureTeam team) => Procedures.Add(IsProcedureTeamTeam1(team) ? MatchProcedure.FreePoint1 : MatchProcedure.FreePoint2);

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
            if (procedureUtilities.CurrentProcedureRequireSelectedMap() && SelectedMap == null)
            {
                SendAlert($"Current operation ({procedureUtilities.GetProcedureDescription(CurrentProcedure)} -> {procedureUtilities.GetProcedureDescription(Procedures[CurrentProcedureIndex + 1])}) requires a map to be specified.");
                return false;
            }
            if (IsFinished)
            {
                SendAlert("Can't progress in a finished match.");
                return false;
            }
            if (CurrentProcedure == MatchProcedure.Rolling && RollWinner == null)
            {
                SendAlert("Roll winner needs to be specified.");
                return false;
            }
            var snapshotName = string.Empty;
            switch (CurrentProcedure)
            {
                case MatchProcedure.SettingUp:
                    snapshotName = "Finish setting up";
                    break;
                case MatchProcedure.WarmUp1:
                    snapshotName = $"Set up team's \"{Team1}\" warmup";
                    break;
                case MatchProcedure.WarmUp2:
                    snapshotName = $"Set up team's \"{Team2}\" warmup";
                    break;
                case MatchProcedure.WarmUpRollWinner:
                    snapshotName = $"Set up team's \"{RollWinner}\" warmup";
                    break;
                case MatchProcedure.WarmUpRollLoser:
                    snapshotName = $"Set up team's \"{RollLoser}\" warmup";
                    break;
                case MatchProcedure.Rolling:
                    snapshotName = $"Set roll winner: {RollWinner}";
                    break;
                case MatchProcedure.Banning1:
                    snapshotName = $"{Team1} bans {SelectedMap}";
                    Team1.BannedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.Banning2:
                    snapshotName = $"{Team2} bans {SelectedMap}";
                    Team2.BannedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.BanningRollWinner:
                    snapshotName = $"{RollWinner} bans {SelectedMap}";
                    RollWinner.BannedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.BanningRollLoser:
                    snapshotName = $"{RollLoser} bans {SelectedMap}";
                    RollLoser.BannedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.Picking1:
                    snapshotName = $"{Team1} picks {SelectedMap}";
                    Team1.PickedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.Picking2:
                    snapshotName = $"{Team2} picks {SelectedMap}";
                    Team2.PickedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.PickingRollWinner:
                    snapshotName = $"{RollWinner} picks {SelectedMap}";
                    RollWinner.PickedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.PickingRollLoser:
                    snapshotName = $"{RollLoser} picks {SelectedMap}";
                    RollLoser.PickedMaps.Add(SelectedMap);
                    break;
                case MatchProcedure.GettingReady:
                    snapshotName = $"Start map: {SelectedMap}";
                    break;
                case MatchProcedure.TieBreaker:
                    snapshotName = $"Set up the tiebreaker";
                    break;
                case MatchProcedure.Playing:
                    snapshotName = $"Finish playing {SelectedMap}";
                    break;
                case MatchProcedure.FreePoint1:
                    snapshotName = $"Free point for {Team1}";
                    Scores[Team1]++;
                    break;
                case MatchProcedure.FreePoint2:
                    snapshotName = $"Free point for {Team2}";
                    Scores[Team2]++;
                    break;
                case MatchProcedure.FreePointRollWinner:
                    snapshotName = $"Free point for {RollWinner}";
                    Scores[RollWinner]++;
                    break;
                case MatchProcedure.FreePointRollLoser:
                    snapshotName = $"Free point for {RollLoser}";
                    Scores[RollLoser]++;
                    break;
            }
            return GoToNextProcedure(snapshotName);
        }

        private bool GoToNextProcedure(string snapshotName)
        {
            CurrentProcedureIndex++;
            DoNewProcedureJobs();
            history.Add(new MatchSnapshot(this, snapshotName));
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

        public void ReverseLastOperation() => Updated?.Invoke();

        private void SendAlert(string message) => Alert.Invoke(this, message);
    }
}
