using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class Match
    {
        private int id = -1;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                Updated();
            }
        }

        private DateTime lastUploadTime;
        public bool ModifiedSinceUpdate => History.Count > 0 && History.Last().Time > lastUploadTime;

        public readonly Team Team1;
        public readonly Team Team2;

        public string Code;

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

        private Team rollWinner;
        public Team RollWinner
        {
            get => rollWinner;
            set
            {
                rollWinner = value;
                Updated?.Invoke();
            }
        }
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
                if (selectedMap == null)
                    history.Add(new MatchSnapshot(this, ReadableCurrentState));
                selectedMap = value;
                Updated();
            }
        }

        public DateTime MapStartTime { get; private set; }
        public double? MapProgress => SelectedMap == null || !(CurrentProcedure == MatchProcedure.Playing || CurrentProcedure == MatchProcedure.PlayingWarmUp) ? null :
            SelectedMap.Length > 0 ? mapPlayTime / mapLength : 1;
        public string MapProgressText => SelectedMap == null ? null :
            $@"{TimeSpan.FromSeconds(mapPlayTime.Value):m\:ss}/{TimeSpan.FromSeconds(mapLength.Value):m\:ss}";

        private bool isDoubleTime => TournamentStage.Mappool.DoubleTime.Contains(SelectedMap);
        private double? mapPlayTime => (DateTime.UtcNow - MapStartTime).TotalSeconds * (isDoubleTime ? 3 / 2d : 1);
        private double? mapLength => SelectedMap != null ? SelectedMap.Length * (isDoubleTime ? 2 / 3d : 1d) : (double?)null;

        private Team selectedWinner;
        public Team SelectedWinner
        {
            get => selectedWinner;
            set
            {
                if (selectedWinner == null)
                    history.Add(new MatchSnapshot(this, ReadableCurrentState));
                selectedWinner = value;
                Updated();
            }
        }

        public Dictionary<Team, int> Scores = new Dictionary<Team, int>();

        public readonly Dictionary<Map, IReadOnlyDictionary<Player, int>> MapResults = new Dictionary<Map, IReadOnlyDictionary<Player, int>>();

        public string Title => TournamentStage.RoomName.Replace("TEAM1", Team1.TeamName).Replace("TEAM2", Team2.TeamName);

        public Match(Team team1, Team team2, Tournament tournament, TournamentStage tournamentStage) : this()
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

        public Match() { }

        public void NotifyAboutUpload()
        {
            lastUploadTime = DateTime.UtcNow;
            Updated();
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
            var snapshot = (MatchSnapshot)null;
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
                    snapshot = new MatchSnapshot(this, null);
                    BanMap(Team1, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.Banning2:
                    snapshot = new MatchSnapshot(this, null);
                    BanMap(Team2, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.BanningRollWinner:
                    snapshot = new MatchSnapshot(this, null);
                    BanMap(RollWinner, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.BanningRollLoser:
                    snapshot = new MatchSnapshot(this, null);
                    BanMap(RollLoser, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.Picking1:
                    snapshot = new MatchSnapshot(this, null);
                    PickMap(Team1, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.Picking2:
                    snapshot = new MatchSnapshot(this, null);
                    PickMap(Team2, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.PickingRollWinner:
                    snapshot = new MatchSnapshot(this, null);
                    PickMap(RollWinner, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.PickingRollLoser:
                    snapshot = new MatchSnapshot(this, null);
                    PickMap(RollLoser, out snapshotName);
                    snapshot.Name = snapshotName;
                    break;
                case MatchProcedure.GettingReady:
                    snapshotName = $"Start map: {SelectedMap}";
                    break;
                case MatchProcedure.TieBreaker:
                    snapshotName = $"Set up the tiebreaker";
                    break;
                case MatchProcedure.Playing:
                    return FinishPlaying();
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
            history.Add(snapshot ?? new MatchSnapshot(this, snapshotName));
            return GoToNextProcedure();
        }

        private bool GoToNextProcedure()
        {
            CurrentProcedureIndex++;
            DoNewProcedureJobs();
            Updated?.Invoke();
            return true;
        }

        private void DoNewProcedureJobs()
        {
            if (CurrentProcedure == MatchProcedure.Rolling)
                RollWinner = null;
            if (CurrentProcedure == MatchProcedure.Playing || CurrentProcedure == MatchProcedure.PlayingWarmUp)
                MapStartTime = DateTime.UtcNow;
        }

        public void ReverseLastOperation()
        {
            if (History.Count == 0)
                return;
            var lastSnapshot = History.Last();

            CurrentProcedureIndex = lastSnapshot.ProcedureIndex;
            Scores[Team1] = lastSnapshot.Team1Score;
            Scores[Team2] = lastSnapshot.Team2Score;
            Team1.PickedMaps = lastSnapshot.T1PickedMaps.ToList();
            Team2.PickedMaps = lastSnapshot.T2PickedMaps.ToList();
            Team1.BannedMaps = lastSnapshot.T1BannedMaps.ToList();
            Team2.BannedMaps = lastSnapshot.T2BannedMaps.ToList();
            rollWinner = lastSnapshot.RollWinnerTeamName == Team1.TeamName ? Team1 :
                lastSnapshot.RollWinnerTeamName == Team2.TeamName ? Team2 : null;
            selectedMap = lastSnapshot.CurrentMap;
            selectedWinner = lastSnapshot.CurrentWinner;

            cancelledOperations.Add(lastSnapshot);
            history.Remove(lastSnapshot);

            Updated?.Invoke();
        }

        private void SendAlert(string message) => Alert.Invoke(this, message);

        private void BanMap(Team team, out string snapshotName)
        {
            snapshotName = $"{team} bans {SelectedMap}";
            team.BannedMaps.Add(SelectedMap);
            selectedMap = null;
        }

        private void PickMap(Team team, out string snapshotName)
        {
            snapshotName = $"{team} picks {SelectedMap}";
            team.PickedMaps.Add(SelectedMap);
        }

        private bool FinishPlaying()
        {
            if (CurrentProcedure != MatchProcedure.Playing)
                return false;
            history.Add(new MatchSnapshot(this, $"{SelectedWinner} wins {SelectedMap}"));
            Scores[SelectedWinner]++;
            selectedMap = null;
            selectedWinner = null;
            var retValue = GoToNextProcedure();
            return retValue;
        }

        private bool FinishWarmUp()
        {
            if (CurrentProcedure != MatchProcedure.PlayingWarmUp)
                return false;
            var snapshotName = $"Finished playing warmup {SelectedMap}";
            selectedMap = null;
            history.Add(new MatchSnapshot(this, snapshotName));
            return GoToNextProcedure();
        }

        public APIMatch GenerateAPIMatch()
        {
            var apiMatch = new APIMatch
            {
                Code = Code,
                History = History.ToList(),
                Id = Id,
                MapResults = new List<MapResult>(),
                RollWinnerTeamName = RollWinner?.TeamName,
                Team1 = new APITeam
                {
                    BannedMaps = Team1.BannedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    PickedMaps = Team1.PickedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    Members = Team1.Members.Select(m => new APIPlayer
                    {
                        PlayerId = m.Id.Value
                    }).ToList(),
                    Score = Scores[Team1],
                    TeamName = Team1.TeamName
                },
                Team2 = new APITeam
                {
                    BannedMaps = Team2.BannedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    PickedMaps = Team2.PickedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    Members = Team2.Members.Select(m => new APIPlayer
                    {
                        PlayerId = m.Id.Value
                    }).ToList(),
                    Score = Scores[Team2],
                    TeamName = Team2.TeamName
                }
            };
            foreach (var finishedMap in MapResults)
            {
                var scores = new Dictionary<int, int>();

                foreach (var score in finishedMap.Value)
                    scores.Add(score.Key.Id.Value, score.Value);

                apiMatch.MapResults.Add(new MapResult
                {
                    DifficultyId = finishedMap.Key.DifficultyId.Value,
                    PlayerScores = scores
                });
            }
            return apiMatch;
        }
    }
}
