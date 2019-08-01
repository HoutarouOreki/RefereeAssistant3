using RefereeAssistant3.Main.IRC;
using RefereeAssistant3.Main.Online.APIModels;
using RefereeAssistant3.Main.Online.APIRequests;
using RefereeAssistant3.Main.Tournaments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main.Matches
{
    public abstract class OsuMatch
    {
        public event Action Updated;
        public event Action<OsuMatch, string> Alert;

        public int Id { get; private set; } = -1;

        public DateTime CreationDate = DateTime.UtcNow;
        public int? RoomId { get; set; }
        public MpRoomIrcChannel IrcChannel { get; set; }
        public string ChannelName => $"#mp_{RoomId}";
        public MpRoomSettings MpRoomSettings { get; set; }

        public string Code;
        public readonly Tournament Tournament;
        public readonly TournamentStageConfiguration TournamentStage;

        public abstract MatchProcedureTypes CurrentProcedureType { get; }
        public abstract string CurrentProcedureName { get; }

        public abstract bool IsFinished { get; }
        public abstract string WinnerName { get; }

        private DateTime? lastUploadTime;
        public bool ModifiedSinceUpdate => History.Count > 0 && History[History.Count - 1].Time > lastUploadTime;

        protected readonly List<OsuMatchSnapshot> Snapshots = new List<OsuMatchSnapshot>();
        public IReadOnlyList<OsuMatchSnapshot> History => Snapshots;
        protected readonly List<OsuMatchSnapshot> RevertedSnapshots = new List<OsuMatchSnapshot>();
        public IReadOnlyList<OsuMatchSnapshot> CancelledOperations => RevertedSnapshots;

        protected List<Player> Players = new List<Player>();

        public abstract IEnumerable<Map> UsedMaps { get; }

        public DateTime MapStartTime { get; protected set; }
        public double? MapProgress => SelectedMap == null || !(CurrentProcedureType == MatchProcedureTypes.Playing || CurrentProcedureType == MatchProcedureTypes.PlayingWarmUp) ? null :
            SelectedMap.Length > 0 ? mapPlayTime / mapLength : 1;
        public string MapProgressText => SelectedMap == null ? null :
            $@"{TimeSpan.FromSeconds(mapPlayTime.Value):m\:ss}/{TimeSpan.FromSeconds(mapLength.Value):m\:ss}";

        private bool isDoubleTime => TournamentStage.Mappool.DoubleTime.Contains(SelectedMap);
        private double? mapPlayTime => (DateTime.UtcNow - MapStartTime).TotalSeconds * (isDoubleTime ? 3 / 2d : 1);
        private double? mapLength => SelectedMap != null ? SelectedMap.Length * (isDoubleTime ? 2 / 3d : 1d) : (double?)null;

        private Map selectedMap;

        /// <summary>
        /// Use <see cref="SetMapFromId(int)"/> to set the map.
        /// </summary>
        public Map SelectedMap
        {
            get => selectedMap;
            private set
            {
                selectedMap = value;
                SignalChanges();
            }
        }

        public BanchoIrcManager BanchoIrc { get; set; }

        public virtual string RoomName => TournamentStage.RoomSettings.RoomName;

        protected OsuMatch(Tournament tournament, TournamentStageConfiguration tournamentStage)
        {
            Tournament = tournament;
            TournamentStage = tournamentStage;

            IrcChannel?.RefreshTimeOutTime();
        }

        protected OsuMatch(APIMatch apiMatch, Tournament tournament, TournamentStageConfiguration tournamentStage) : this(tournament, tournamentStage)
        {
            Id = apiMatch.Id;
            Snapshots = apiMatch.History;
            IrcChannel = apiMatch.Chat;
            Code = apiMatch.Code;
            foreach (var p in apiMatch.Players)
            {
                var player = GetPlayer(p.PlayerId);
                if (player != null)
                    player.MapResults = p.MapResults;
            }
        }

        /// <summary>
        /// Uses the <see cref="GetMap(int)"/> function internally and sets it as <see cref="SelectedMap"/>.
        /// </summary>
        public void SetMapFromId(int? difficultyId)
        {
            if (!difficultyId.HasValue)
            {
                SelectedMap = null;
                return;
            }
            SelectedMap = GetMap(difficultyId.Value);
            if (!SelectedMap.Downloaded)
                SelectedMap.DownloadDataAsync().ContinueWith(t => Updated());
        }

        /// <summary>
        /// Returns a cached map from the mappool, or - if it's not found - creates a new one and starts downloading its data.
        /// </summary>
        public Map GetMap(int difficultyId)
        {
            var map = TournamentStage.Mappool.AllMaps.Find(m => m.DifficultyId == difficultyId) ?? new Map(difficultyId);
            Task.Run(() => map.DownloadDataAsync());
            return map;
        }

        public void SetPlayerScoreOnCurrentMap(string username, int score, bool passed)
        {
            if (SelectedMap == null)
            {
                SendAlert("Setting a player score requires a map to be selected");
                return;
            }
            var player = GetPlayer(username);
            player.MapResults.Add(new PlayerMapResult(SelectedMap.DifficultyId, passed, score, player.SelectedMods));
        }

        public void FinishMatch()
        {
            IrcChannel?.RefreshTimeOutTime();
            OnMatchFinished();
        }

        public abstract APIMatch GenerateAPIMatch();

        /// <summary>
        /// This function is called by <see cref="Core"/> after creating the match and after rolling.
        /// </summary>
        public abstract void GenerateMatchProcedures();

        public abstract bool Proceed();

        public abstract void ReverseLastOperation();

        /// <summary>
        /// This function will be ran by <see cref="Core"/> after this match is constructed.
        /// Inside it all participating players' info is downloaded (usernames, ids, etc).
        /// This is required for properly recognizing participants.
        /// </summary>
        public virtual async Task PreparePlayersInfo() => await Task.WhenAll(Players.Select(p => p.DownloadMetadata()));

        public async Task UpdateMatchAsync()
        {
            var req = await new PutMatchUpdate(GenerateAPIMatch()).RunTask();
            if (req?.Response?.IsSuccessful == true)
                lastUploadTime = DateTime.UtcNow;
            else
                SendAlert($"Updating match {Code} failed with code {req?.Response?.StatusCode}:\n{req?.Response?.Content}");
        }

        public async Task PostMatchAsync()
        {
            Id = -2;
            var req = await new PostNewMatch(GenerateAPIMatch()).RunTask();
            if (req?.Response?.IsSuccessful == true)
            {
                Id = req.Object.Id;
                SendAlert($"Match {req.Object.Code} posted successfully");
                lastUploadTime = DateTime.UtcNow;
            }
            else
            {
                Id = -1;
                SendAlert($"Failed to post match {Code}, code {req?.Response?.StatusCode}\n{req?.Response?.ErrorMessage}\n{req?.Response?.Content}");
            }
        }

        public virtual Player GetPlayer(int playerId)
        {
            var player = Players.Find(p => p.PlayerId == playerId);
            if (player == null)
                Players.Add(player = new Player(playerId));
            Task.Run(player.DownloadMetadata);
            return player;
        }

        public virtual Player GetPlayer(string username)
        {
            var player = Players.Find(p => p.Equals(username));
            if (player == null)
                Players.Add(player = new Player(username));
            Task.Run(player.DownloadMetadata);
            return player;
        }

        /// <summary>
        /// Use this function to fire the <see cref="Updated"/> event.
        /// </summary>
        protected void SignalChanges() => Updated?.Invoke();

        protected void SendAlert(string message) => Alert.Invoke(this, message);

        /// <summary>
        /// Snapshots created by this function will be saved in <see cref="History"/>.
        /// </summary>
        protected abstract OsuMatchSnapshot CreateSnapshot();

        protected abstract void SetStateFromSnapshot(OsuMatchSnapshot matchSnapshot);

        protected abstract void OnMatchFinished();
    }

    public abstract class OsuMatch<TParticipant> : OsuMatch where TParticipant : MatchParticipant
    {
        public List<MatchProcedure<TParticipant>> Procedures = new List<MatchProcedure<TParticipant>>();
        public MatchProcedure<TParticipant> CurrentProcedure => Procedures.ElementAtOrDefault(CurrentProcedureIndex) ?? new MatchProcedure<TParticipant>(MatchProcedureTypes.SettingUp);
        public override MatchProcedureTypes CurrentProcedureType => CurrentProcedure.ProcedureType;
        public override string CurrentProcedureName => CurrentProcedure.Name;
        public int CurrentProcedureIndex { get; protected set; }

        public override bool IsFinished => Participants.Any(participant => Scores[participant] == TournamentStage.ScoreRequiredToWin);

        protected List<TParticipant> Participants = new List<TParticipant>();
        public IEnumerable<TParticipant> MatchParticipants => Participants;

        public override IEnumerable<Map> UsedMaps => Participants.SelectMany(participant => participant.PickedMaps.Concat(participant.BannedMaps));

        public readonly Dictionary<Map, IReadOnlyDictionary<APIPlayer, int>> MapResults = new Dictionary<Map, IReadOnlyDictionary<APIPlayer, int>>();

        private TParticipant selectedWinner;
        public TParticipant SelectedWinner
        {
            get => selectedWinner;
            set
            {
                selectedWinner = value;
                SignalChanges();
            }
        }

        private TParticipant rollWinner;
        public TParticipant RollWinner
        {
            get => rollWinner;
            set
            {
                rollWinner = value;
                SignalChanges();
            }
        }

        public Dictionary<TParticipant, int> Scores = new Dictionary<TParticipant, int>();

        public OsuMatch(Tournament tournament, TournamentStageConfiguration tournamentStage) : base(tournament, tournamentStage) { }

        public OsuMatch(APIMatch apiMatch, Tournament tournament, TournamentStageConfiguration tournamentStage) : base(apiMatch, tournament, tournamentStage) { }

        public override void GenerateMatchProcedures()
        {
            Procedures.Clear();
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.SettingUp));
            if (Snapshots.Count == 0)
                Snapshots.Add(CreateSnapshot());
            foreach (var procedureParameter in TournamentStage.MatchProceedings)
                GenerateMatchProcedure(procedureParameter);
        }

        public override bool Proceed()
        {
            if (new[] { MatchProcedureTypes.Banning, MatchProcedureTypes.GettingReady, MatchProcedureTypes.Picking, MatchProcedureTypes.Playing, MatchProcedureTypes.PlayingWarmUp, MatchProcedureTypes.WarmUp }.Contains(CurrentProcedure.ProcedureType) && SelectedMap == null)
            {
                SendAlert($"Current procedure ({CurrentProcedure.Name}) requires a map to be specified.");
                return false;
            }
            if (IsFinished)
            {
                SendAlert("Can't progress in a finished match.");
                return false;
            }
            if (CurrentProcedure.ProcedureType == MatchProcedureTypes.Rolling && RollWinner == null)
            {
                SendAlert("Roll winner needs to be specified.");
                return false;
            }

            switch (CurrentProcedure.ProcedureType)
            {
                case MatchProcedureTypes.SettingUp:
                    break;
                case MatchProcedureTypes.WarmUp:
                    break;
                case MatchProcedureTypes.Rolling:
                    GenerateMatchProcedures();
                    break;
                case MatchProcedureTypes.Banning:
                    CurrentProcedure.Participant.BannedMaps.Add(SelectedMap);
                    SetMapFromId(null);
                    break;
                case MatchProcedureTypes.Picking:
                    CurrentProcedure.Participant.PickedMaps.Add(SelectedMap);
                    BanchoIrc?.SetMap(this, SelectedMap, PlayMode.osu);
                    break;
                case MatchProcedureTypes.GettingReady:
                    BanchoIrc?.StartMatch(this, 10);
                    break;
                case MatchProcedureTypes.TieBreaker:
                    break;
                case MatchProcedureTypes.Playing:
                    if (SelectedWinner == null)
                    {
                        SendAlert("Proceeding requires a winner to be specified");
                        return false;
                    }
                    IrcChannel?.RefreshTimeOutTime();
                    Scores[SelectedWinner]++;
                    selectedWinner = null;
                    SetMapFromId(null);
                    break;
                case MatchProcedureTypes.PlayingWarmUp:
                    IrcChannel?.RefreshTimeOutTime();
                    SetMapFromId(null);
                    break;
                case MatchProcedureTypes.FreePoint:
                    Scores[CurrentProcedure.Participant]++;
                    break;
                default:
                    break;
            }
            return GoToNextProcedure();
        }

        public override void ReverseLastOperation()
        {
            if (History.Count <= 1)
                return;
            var lastSnapshot = History[History.Count - 1];
            RevertedSnapshots.Add(lastSnapshot);
            Snapshots.Remove(lastSnapshot);
            lastSnapshot = History[History.Count - 1];
            SetStateFromSnapshot(lastSnapshot);
            SignalChanges();
        }

        public override APIMatch GenerateAPIMatch()
        {
            var apiMatch = new APIMatch
            {
                Code = Code,
                History = History.ToList(),
                Id = Id,
                Players = new List<APIPlayer>(Players.Select(p => new APIPlayer(p.PlayerId.Value) { MapResults = p.MapResults })),
                Chat = IrcChannel,
                Participants = new List<APIParticipant>(Participants.Select(p => new APIParticipant(p, Scores[p]))),
                TournamentName = Tournament.Configuration.TournamentName,
                TournamentStage = TournamentStage.TournamentStageName,
            };
            return apiMatch;
        }

        /// <summary>
        /// This function is used for parsing strings from <see cref="TournamentStage.MatchProceedings"/>.
        /// <see cref="MatchProcedureTypes.GettingReady"/>, <see cref="MatchProcedureTypes.Playing"/>,
        /// <see cref="MatchProcedureTypes.PlayingWarmUp"/> and <see cref="MatchProcedureTypes.SettingUp"/>
        /// are not to be returned as they're automatically generated and will not be processed.
        /// </summary>
        /// <param name="procedureString">Always lower case.</param>
        protected abstract (MatchProcedureTypes procedureType, TParticipant participant) ParseProcedure(string procedureString);

        private bool GoToNextProcedure()
        {
            CurrentProcedureIndex++;
            DoNewProcedureJobs();
            Snapshots.Add(CreateSnapshot());
            SignalChanges();
            return true;
        }

        private void DoNewProcedureJobs()
        {
            if (CurrentProcedure.ProcedureType == MatchProcedureTypes.Rolling)
                RollWinner = null;
            if (CurrentProcedure.ProcedureType == MatchProcedureTypes.Playing || CurrentProcedure.ProcedureType == MatchProcedureTypes.PlayingWarmUp)
                MapStartTime = DateTime.UtcNow;
        }

        private void GenerateMatchProcedure(string param)
        {
            var (procedureType, participant) = ParseProcedure(param.ToLowerInvariant());
            switch (procedureType)
            {
                case MatchProcedureTypes.WarmUp:
                    GenerateWarmupProcedure(participant);
                    break;
                case MatchProcedureTypes.Rolling:
                    GenerateRollingProcedure();
                    break;
                case MatchProcedureTypes.Banning:
                    GenerateBanningProcedure(participant);
                    break;
                case MatchProcedureTypes.Picking:
                    GeneratePickingProcedure(participant);
                    break;
                case MatchProcedureTypes.TieBreaker:
                    GenerateTiebreakerProcedure();
                    break;
                case MatchProcedureTypes.FreePoint:
                    GenerateFreePointProcedure(participant);
                    break;
                default:
                    break;
            }
        }

        private void GenerateWarmupProcedure(TParticipant warmupProvider)
        {
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.WarmUp, warmupProvider));
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.GettingReady));
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.PlayingWarmUp));
        }

        private void GenerateRollingProcedure() => Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.Rolling));

        private void GeneratePickingProcedure(TParticipant picker)
        {
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.Picking, picker));
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.GettingReady));
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.Playing));
        }

        private void GenerateBanningProcedure(TParticipant banner) => Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.Banning, banner));

        private void GenerateTiebreakerProcedure()
        {
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.TieBreaker));
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.GettingReady));
            Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.Playing));
        }

        private void GenerateFreePointProcedure(TParticipant receiver) => Procedures.Add(new MatchProcedure<TParticipant>(MatchProcedureTypes.FreePoint, receiver));
    }
}
