using RefereeAssistant3.Main.Online.APIModels;
using RefereeAssistant3.Main.Tournaments;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main.Matches
{
    public class OsuHeadToHeadMatch : OsuMatch<Player>
    {
        private readonly APIMatch apiMatch;

        public override string WinnerName => Participants.Find(p => Scores[p] == TournamentStage.ScoreRequiredToWin)?.Name;

        public OsuHeadToHeadMatch(Tournament tournament, TournamentStageConfiguration tournamentStage, params APIPlayer[] players) : base(tournament, tournamentStage)
        {
            Players.AddRange(players.Select(p => new Player(p.PlayerId)));
            foreach (var player in Players)
                Scores.Add(player, 0);
        }

        public OsuHeadToHeadMatch(APIMatch apiMatch, Tournament tournament, TournamentStageConfiguration tournamentStage) : base(apiMatch, tournament, tournamentStage)
        {
            Players.AddRange(apiMatch.Players.Select(p => new Player(p.PlayerId)));
            apiMatch.Players.Select(p => Players.Find(player => player.PlayerId == p.PlayerId).MapResults = p.MapResults);
            this.apiMatch = apiMatch; /// need to assign scores after loading all players in <see cref="PreparePlayersInfo"/>
        }

        public override async Task PreparePlayersInfo()
        {
            await base.PreparePlayersInfo();
            if (apiMatch == null)
                return;
            foreach (var player in Players)
            {
                var apiParticipant = apiMatch.Participants.First(p => p.Name == player.Name);
                Scores[player] = apiParticipant.Score;
                player.BannedMaps = apiParticipant.BannedMaps.Select(m => GetMap(m)).ToList();
                player.PickedMaps = apiParticipant.PickedMaps.Select(m => GetMap(m)).ToList();
            }
        }

        protected override OsuMatchSnapshot CreateSnapshot() => new OsuMatchSnapshot
        {
            Name = CurrentProcedureName,
            Participants = Participants.Select(p => new APIParticipant(p, Scores[p])).ToList(),
            Players = Participants.Select(p => new APIPlayer(p.PlayerId.Value) { MapResults = p.MapResults }).ToList(),
            ProcedureIndex = CurrentProcedureIndex,
            RollWinnersName = RollWinner?.Name,
            SelectedMap = SelectedMap?.DifficultyId,
            SelectedWinner = SelectedWinner != null ? new APIParticipant(SelectedWinner, Scores[SelectedWinner]) : null
        };

        protected override void SetStateFromSnapshot(OsuMatchSnapshot matchSnapshot)
        {
            foreach (var participant in matchSnapshot.Participants)
            {
                var player = Participants.Find(p => p.Name == participant.Name);
                player.BannedMaps = participant.BannedMaps.Select(m => GetMap(m)).ToList();
                player.PickedMaps = participant.PickedMaps.Select(m => GetMap(m)).ToList();
                Scores[player] = participant.Score;
            }
            foreach (var apiPlayer in matchSnapshot.Players)
            {
                var player = Participants.Find(p => p.PlayerId == apiPlayer.PlayerId);
                player.MapResults = apiPlayer.MapResults;
            }
            CurrentProcedureIndex = matchSnapshot.ProcedureIndex;
            RollWinner = Participants.Find(p => p.Name == matchSnapshot.RollWinnersName);
            SetMapFromId(matchSnapshot.SelectedMap);
            SelectedWinner = Participants.Find(p => p.Name == matchSnapshot.SelectedWinner.Name);
        }

        protected override void OnMatchFinished() => throw new System.NotImplementedException();
        protected override (MatchProcedureTypes procedureType, Player participant) ParseProcedure(string procedureString) => throw new System.NotImplementedException();
    }
}
