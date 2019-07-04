using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public DateTime CreationDate = DateTime.UtcNow;

        public int? RoomId;
        public string ChannelName => RoomId.HasValue ? $"#mp_{RoomId}" : null;
        public MpRoomIrcChannel IrcChannel;

        public MatchSettings LastReadSettings { get; set; }

        private DateTime lastUploadTime;
        public bool ModifiedSinceUpdate => History.Count > 0 && History.Last().Time > lastUploadTime;

        public DateTime TimeOutTime { get; private set; }

        public readonly Team Team1;
        public readonly Team Team2;

        public string Code;

        private readonly List<MatchSnapshot> history = new List<MatchSnapshot>();
        public IReadOnlyList<MatchSnapshot> History => history;
        private readonly List<MatchSnapshot> cancelledOperations = new List<MatchSnapshot>();
        public IReadOnlyList<MatchSnapshot> CancelledOperations => cancelledOperations;

        public event Action Updated;
        public event Action<Match, string> Alert;

        public OsuIrcBot ChatBot { get; }

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
                selectedWinner = value;
                Updated();
            }
        }

        public Dictionary<Team, int> Scores = new Dictionary<Team, int>();

        public readonly Dictionary<Map, IReadOnlyDictionary<Player, int>> MapResults = new Dictionary<Map, IReadOnlyDictionary<Player, int>>();

        public string RoomName => TournamentStage.RoomName.Replace("TEAM1", Team1.TeamName).Replace("TEAM2", Team2.TeamName);

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
            GenerateMatchProcedures();
            history.Add(new MatchSnapshot(this, ReadableCurrentState));

            RefreshTimeOut();
        }

        public Match(Core core, APIMatch apiMatch)
        {
            Tournament = core.Tournaments.Find(t => t.Configuration.TournamentName == apiMatch.TournamentName);
            TournamentStage = Tournament.Stages.Find(s => s.TournamentStageName == apiMatch.TournamentStage);
            procedureUtilities = new ProcedureUtilities(this);
            GenerateMatchProcedures();
            history = apiMatch.History;
            Team1 = new Team(apiMatch.Team1);
            Scores[Team1] = apiMatch.Team1.Score;
            Team2 = new Team(apiMatch.Team2);
            Scores[Team2] = apiMatch.Team2.Score;
            if (history.Count > 0)
            {
                Team1.PickedMaps = apiMatch.History.Last().Team1PickedMaps.Select(id => GetMap(id)).ToList();
                Team1.BannedMaps = apiMatch.History.Last().Team1BannedMaps.Select(id => GetMap(id)).ToList();
                Team2.PickedMaps = apiMatch.History.Last().Team2PickedMaps.Select(id => GetMap(id)).ToList();
                Team2.BannedMaps = apiMatch.History.Last().Team2BannedMaps.Select(id => GetMap(id)).ToList();
                var sWinner = history.Last().SelectedWinner;
                selectedWinner = string.IsNullOrEmpty(sWinner) ? null : sWinner == Team1.TeamName ? Team1 : Team2;
                var sMap = history.Last().SelectedMap;
                if (sMap != null)
                    selectedMap = GetMap(sMap.Value);
                CurrentProcedureIndex = apiMatch.History.Last().ProcedureIndex;
                var rWinner = apiMatch.History.Last().RollWinnerTeamName;
                RollWinner = rWinner == Team1.TeamName ? Team1
                    : rWinner == apiMatch.Team2.TeamName ? Team2 : null;
            }
            selectedMap?.DownloadDataAsync();
            IrcChannel = apiMatch.Chat;
            Code = apiMatch.Code;
            id = apiMatch.Id;
            foreach (var mapResult in apiMatch.MapResults)
            {
                MapResults.Add(new Map(mapResult.DifficultyId), new Dictionary<Player, int>(mapResult.PlayerScores.Select(kv => new KeyValuePair<Player, int>(new Player(kv.Key), kv.Value))));
            }
        }

        private Map GetMap(int diffId)
        {
            var map = TournamentStage.Mappool.AllMaps.FirstOrDefault(m => m.DifficultyId == diffId) ?? new Map(diffId);
            Task.Run(() => map.DownloadDataAsync());
            return map;
        }

        private void RefreshTimeOut() => TimeOutTime = DateTime.UtcNow + TimeSpan.FromMinutes(30);

        public void NotifyAboutUpload()
        {
            lastUploadTime = DateTime.UtcNow;
            Updated();
        }

        public void SelectMapFromId(int difficultyId)
        {
            var map = TournamentStage.Mappool.AllMaps.FirstOrDefault(m => m.DifficultyId == difficultyId);
            SelectedMap = map ?? new Map(difficultyId);
            if (!SelectedMap.Downloaded)
                SelectedMap.DownloadDataAsync().ContinueWith(t => Updated());
        }

        public Player GetPlayer(string username) => Team1.Members.Concat(Team2.Members).FirstOrDefault(p => p.IRCUsername == username || p.Username == username);

        private void GenerateMatchProcedures()
        {
            Procedures.Clear();
            Procedures.Add(MatchProcedure.SettingUp);
            foreach (var procedureParameter in TournamentStage.MatchProceedings)
                GenerateMatchProcedure(procedureParameter);
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
            switch (CurrentProcedure)
            {
                case MatchProcedure.SettingUp:                    break;
                case MatchProcedure.WarmUp1:                    break;
                case MatchProcedure.WarmUp2:                    break;
                case MatchProcedure.WarmUpRollWinner:                    break;
                case MatchProcedure.WarmUpRollLoser:                    break;
                case MatchProcedure.Rolling:                    break;
                case MatchProcedure.Banning1:                    BanMap(Team1);
                    break;
                case MatchProcedure.Banning2:                    BanMap(Team2);
                    break;
                case MatchProcedure.BanningRollWinner:                    BanMap(RollWinner);
                    break;
                case MatchProcedure.BanningRollLoser:                    BanMap(RollLoser);
                    break;
                case MatchProcedure.Picking1:                    PickMap(Team1);
                    break;
                case MatchProcedure.Picking2:                    PickMap(Team2);
                    break;
                case MatchProcedure.PickingRollWinner:                    PickMap(RollWinner);
                    break;
                case MatchProcedure.PickingRollLoser:                    PickMap(RollLoser);
                    break;
                case MatchProcedure.GettingReady:                    break;
                case MatchProcedure.TieBreaker:                    break;
                case MatchProcedure.Playing:
                    return FinishPlaying();
                case MatchProcedure.PlayingWarmUp:
                    return FinishWarmUp();
                case MatchProcedure.FreePoint1:                    Scores[Team1]++;
                    break;
                case MatchProcedure.FreePoint2:                    Scores[Team2]++;
                    break;
                case MatchProcedure.FreePointRollWinner:                    Scores[RollWinner]++;
                    break;
                case MatchProcedure.FreePointRollLoser:                    Scores[RollLoser]++;
                    break;
            }
            return GoToNextProcedure();
        }

        private bool GoToNextProcedure()
        {
            CurrentProcedureIndex++;
            DoNewProcedureJobs();
            history.Add(new MatchSnapshot(this, ReadableCurrentState));
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
            if (History.Count <= 1)
                return;
            var lastSnapshot = History.Last();
            cancelledOperations.Add(lastSnapshot);
            history.Remove(lastSnapshot);
            lastSnapshot = History.Last();

            CurrentProcedureIndex = lastSnapshot.ProcedureIndex;
            Scores[Team1] = lastSnapshot.Team1Score;
            Scores[Team2] = lastSnapshot.Team2Score;
            Team1.PickedMaps = lastSnapshot.Team1PickedMaps.Select(m => GetMap(m)).ToList();
            Team2.PickedMaps = lastSnapshot.Team2PickedMaps.Select(m => GetMap(m)).ToList();
            Team1.BannedMaps = lastSnapshot.Team1BannedMaps.Select(m => GetMap(m)).ToList();
            Team2.BannedMaps = lastSnapshot.Team2BannedMaps.Select(m => GetMap(m)).ToList();
            rollWinner = lastSnapshot.RollWinnerTeamName == Team1.TeamName ? Team1 :
                lastSnapshot.RollWinnerTeamName == Team2.TeamName ? Team2 : null;
            selectedMap = lastSnapshot.SelectedMap.HasValue ? GetMap(lastSnapshot.SelectedMap.Value) : null;
            selectedWinner = !string.IsNullOrEmpty(lastSnapshot.SelectedWinner) ? (Team1.TeamName == lastSnapshot.SelectedWinner ? Team1 : Team2) : null;

            Updated?.Invoke();
        }

        private void SendAlert(string message) => Alert.Invoke(this, message);

        private void BanMap(Team team)
        {            team.BannedMaps.Add(SelectedMap);
            selectedMap = null;
        }

        private void PickMap(Team team) => team.PickedMaps.Add(SelectedMap);

        private bool FinishPlaying()
        {
            if (CurrentProcedure != MatchProcedure.Playing || SelectedWinner == null)
                return false;
            RefreshTimeOut();
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
            RefreshTimeOut();            selectedMap = null;
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
                Chat = IrcChannel,
                Team1 = new APITeam
                {
                    BannedMaps = Team1.BannedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    PickedMaps = Team1.PickedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    Members = Team1.Members.Select(m => new APIPlayer(m.PlayerId.Value)).ToList(),
                    Score = Scores[Team1],
                    TeamName = Team1.TeamName
                },
                Team2 = new APITeam
                {
                    BannedMaps = Team2.BannedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    PickedMaps = Team2.PickedMaps.Select(m => m.DifficultyId.Value).ToList(),
                    Members = Team2.Members.Select(m => new APIPlayer(m.PlayerId.Value)).ToList(),
                    Score = Scores[Team2],
                    TeamName = Team2.TeamName
                },
                TournamentName = Tournament.Configuration.TournamentName,
                TournamentStage = TournamentStage.TournamentStageName
            };
            foreach (var finishedMap in MapResults)
            {
                var scores = new Dictionary<int, int>();

                foreach (var score in finishedMap.Value)
                    scores.Add(score.Key.PlayerId.Value, score.Value);

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
