using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class Match
    {
        public readonly Team Team1;
        public readonly Team Team2;

        public readonly List<ReversibleOperation> History = new List<ReversibleOperation>(); // in case we need to undo anything

        public event Action Updated;

        public readonly Mappool Mappool;

        public readonly Tournament Tournament;
        public readonly TournamentStage TournamentStage;

        public List<MatchProcedure> Procedures = new List<MatchProcedure>();
        private readonly ProcedureUtilities procedureUtilities;

        public MatchProcedure CurrentProcedure => Procedures[currentProcedureIndex];
        private int currentProcedureIndex;

        public Team RollWinner;
        public Team RollLoser => RollWinner == null ? null :
            Team1 == RollWinner ? Team2 : Team1;

        public Map SelectedMap;

        public Dictionary<Team, int> Score = new Dictionary<Team, int>();

        public readonly Dictionary<Map, Dictionary<Player, int>> MapResults = new Dictionary<Map, Dictionary<Player, int>>();

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

            procedureUtilities = new ProcedureUtilities(this);

            Procedures.Add(MatchProcedure.SettingUp);
            GenerateMatchProcedures();
        }

        private void GenerateMatchProcedures()
        {
            Procedures.Clear();
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
            if (CurrentProcedure == MatchProcedure.SettingUp)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.WarmUp1 || CurrentProcedure == MatchProcedure.WarmUp2)
                return GoToNextProcedure();
            if (CurrentProcedure == MatchProcedure.Rolling && RollWinner != null)
            {
                GenerateMatchProcedures();
                return GoToNextProcedure();
            }
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
}
