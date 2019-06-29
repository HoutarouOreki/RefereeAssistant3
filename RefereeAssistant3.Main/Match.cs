using System;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<Map> UsedMaps => Team1.PickedMaps.Concat(Team1.BannedMaps.Concat(Team2.PickedMaps.Concat(Team2.BannedMaps)));

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

        public Team Winner => Scores[Team1] == TournamentStage.ScoreRequiredToWin ? Team1 :
            Scores[Team2] == TournamentStage.ScoreRequiredToWin ? Team2 : null;

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
            Procedures.Add(MatchProcedure.PlayingWarmUp);
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
                    BanMap(Team1, out snapshotName);
                    break;
                case MatchProcedure.Banning2:
                    BanMap(Team2, out snapshotName);
                    break;
                case MatchProcedure.BanningRollWinner:
                    BanMap(RollWinner, out snapshotName);
                    break;
                case MatchProcedure.BanningRollLoser:
                    BanMap(RollLoser, out snapshotName);
                    break;
                case MatchProcedure.Picking1:
                    PickMap(Team1, out snapshotName);
                    break;
                case MatchProcedure.Picking2:
                    PickMap(Team2, out snapshotName);
                    break;
                case MatchProcedure.PickingRollWinner:
                    PickMap(RollWinner, out snapshotName);
                    break;
                case MatchProcedure.PickingRollLoser:
                    PickMap(RollLoser, out snapshotName);
                    break;
                case MatchProcedure.GettingReady:
                    snapshotName = $"Start map: {SelectedMap}";
                    break;
                case MatchProcedure.TieBreaker:
                    snapshotName = $"Set up the tiebreaker";
                    break;
                case MatchProcedure.Playing:
                    return false;
                case MatchProcedure.PlayingWarmUp:
                    return FinishWarmUp();
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

        private void BanMap(Team team, out string snapshotName)
        {
            snapshotName = $"{team} bans {SelectedMap}";
            team.BannedMaps.Add(SelectedMap);
            SelectedMap = null;
        }

        private void PickMap(Team team, out string snapshotName)
        {
            snapshotName = $"{team} picks {SelectedMap}";
            team.PickedMaps.Add(SelectedMap);
        }

        public bool FinishPlaying(Team winner)
        {
            if (CurrentProcedure != MatchProcedure.Playing)
                return false;
            Scores[winner]++;
            var snapshotName = $"{winner} won {SelectedMap}";
            SelectedMap = null;
            return GoToNextProcedure(snapshotName);
        }

        private bool FinishWarmUp()
        {
            if (CurrentProcedure != MatchProcedure.PlayingWarmUp)
                return false;
            var snapshotName = $"Finished playing warmup {SelectedMap}";
            SelectedMap = null;
            return GoToNextProcedure(snapshotName);
        }
    }
}
