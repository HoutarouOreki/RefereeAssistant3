using RefereeAssistant3.Main.Online.APIModels;
using RefereeAssistant3.Main.Storage;
using RefereeAssistant3.Main.Tournaments;
using System.Linq;

namespace RefereeAssistant3.Main.Matches
{
    public class OsuTeamVsMatch : OsuMatch<Team>
    {
        public Team Team1 => Participants.ElementAtOrDefault(0);

        public Team Team2 => Participants.ElementAtOrDefault(1);

        public Team RollLoser => RollWinner == null ? null :
            Team1 == RollWinner ? Team2 : Team1;

        public Team Winner => Scores[Team1] == TournamentStage.ScoreRequiredToWin ? Team1 :
            Scores[Team2] == TournamentStage.ScoreRequiredToWin ? Team2 : null;

        public override string WinnerName => Winner?.Name;

        public string RoomName => TournamentStage.RoomSettings.RoomName.Replace("TEAM1", Team1.TeamName).Replace("TEAM2", Team2.TeamName);

        public OsuTeamVsMatch(TeamStorage team1, TeamStorage team2, Tournament tournament, TournamentStageConfiguration tournamentStage) : base(tournament, tournamentStage)
        {
            Participants.Add(new Team(team1));
            Participants.Add(new Team(team2));
            Players.AddRange(Team1.Members.Concat(Team2.Members));
            Scores.Add(Team1, 0);
            Scores.Add(Team2, 0);
        }

        public OsuTeamVsMatch(APIMatch apiMatch, TeamStorage team1, TeamStorage team2, Tournament tournament, TournamentStageConfiguration tournamentStage) : base(apiMatch, tournament, tournamentStage)
        {
            Participants.Add(new Team(team1));
            Participants.Add(new Team(team2));
            Players.AddRange(Team1.Members.Concat(Team2.Members));
            Scores[Team1] = apiMatch.Participants[0].Score;
            Scores[Team2] = apiMatch.Participants[1].Score;
            if (Snapshots.Count > 0)
            {
                var lastSnapshot = apiMatch.History[apiMatch.History.Count - 1];
                Team1.PickedMaps = lastSnapshot.Participants[0].PickedMaps.Select(id => GetMap(id)).ToList();
                Team1.BannedMaps = lastSnapshot.Participants[0].BannedMaps.Select(id => GetMap(id)).ToList();
                Team2.PickedMaps = lastSnapshot.Participants[1].PickedMaps.Select(id => GetMap(id)).ToList();
                Team2.BannedMaps = lastSnapshot.Participants[1].BannedMaps.Select(id => GetMap(id)).ToList();
                CurrentProcedureIndex = lastSnapshot.ProcedureIndex;
                var rWinner = lastSnapshot.RollWinnersName;
                RollWinner = rWinner == Team1.TeamName ? Team1 : rWinner == Team2.TeamName ? Team2 : null;
            }
            IrcChannel = apiMatch.Chat;
            Code = apiMatch.Code;
        }

        public override Player GetPlayer(int playerId) => Team1 == null ? null : base.GetPlayer(playerId);

        public override Player GetPlayer(string username) => Team1 == null ? null : base.GetPlayer(username);

        protected override void OnMatchFinished()
        {
            var team1Score = 0;
            var team2Score = 0;
            foreach (var player in Players)
            {
                if (Team1.Members.Any(p => p.Equals(player)))
                    team1Score += player.MapResults.LastOrDefault(mr => mr.DifficultyId == SelectedMap.DifficultyId)?.Score ?? 0;
                if (Team2.Members.Any(p => p.Equals(player)))
                    team2Score += player.MapResults.LastOrDefault(mr => mr.DifficultyId == SelectedMap.DifficultyId)?.Score ?? 0;
            }
            BanchoIrc.SendMessage(IrcChannel, $"{Team1.Name}: {team1Score}");
            BanchoIrc.SendMessage(IrcChannel, $"{Team2.Name}: {team2Score}");
        }

        protected override OsuMatchSnapshot CreateSnapshot() => new OsuMatchSnapshot
        {
            Name = CurrentProcedure.Name,
            Participants = Participants.Select(p => new APIParticipant(p, Scores[p])).ToList(),
            ProcedureIndex = CurrentProcedureIndex,
            RollWinnersName = RollWinner?.Name,
            SelectedMap = SelectedMap?.DifficultyId,
            SelectedWinner = SelectedWinner == null ? null : new APIParticipant(SelectedWinner, Scores[SelectedWinner]),
            Players = Players.Select(p => new APIPlayer(p.PlayerId.Value) { MapResults = p.MapResults }).ToList()
        };

        protected override void SetStateFromSnapshot(OsuMatchSnapshot lastSnapshot)
        {
            CurrentProcedureIndex = lastSnapshot.ProcedureIndex;
            Scores[Team1] = lastSnapshot.Participants[0].Score;
            Scores[Team2] = lastSnapshot.Participants[1].Score;
            Team1.PickedMaps = lastSnapshot.Participants[0].PickedMaps.Select(m => GetMap(m)).ToList();
            Team2.PickedMaps = lastSnapshot.Participants[1].PickedMaps.Select(m => GetMap(m)).ToList();
            Team1.BannedMaps = lastSnapshot.Participants[0].BannedMaps.Select(m => GetMap(m)).ToList();
            Team2.BannedMaps = lastSnapshot.Participants[1].BannedMaps.Select(m => GetMap(m)).ToList();
            RollWinner = lastSnapshot.RollWinnersName == Team1.TeamName ? Team1 :
                lastSnapshot.RollWinnersName == Team2.TeamName ? Team2 : null;
            SetMapFromId(lastSnapshot.SelectedMap);
            if (lastSnapshot.SelectedWinner?.Name == Team1.Name)
                SelectedWinner = Team1;
            else if (lastSnapshot.SelectedWinner?.Name == Team2.Name)
                SelectedWinner = Team2;
            foreach (var p in lastSnapshot.Players)
            {
                var player = GetPlayer(p.PlayerId);
                player.MapResults = p.MapResults;
            }
        }

        protected override (MatchProcedureTypes procedureType, Team participant) ParseProcedure(string procedureString)
        {
            switch (procedureString)
            {
                case "warm1":
                    return (MatchProcedureTypes.WarmUp, Team1);
                case "warm2":
                    return (MatchProcedureTypes.WarmUp, Team2);
                case "warmw":
                    return (MatchProcedureTypes.WarmUp, RollWinner);
                case "warml":
                    return (MatchProcedureTypes.WarmUp, RollLoser);
                case "roll":
                    return (MatchProcedureTypes.Rolling, null);
                case "p1":
                    return (MatchProcedureTypes.Picking, Team1);
                case "p2":
                    return (MatchProcedureTypes.Picking, Team2);
                case "pw":
                    return (MatchProcedureTypes.Picking, RollWinner);
                case "pl":
                    return (MatchProcedureTypes.Picking, RollLoser);
                case "b1":
                    return (MatchProcedureTypes.Banning, Team1);
                case "b2":
                    return (MatchProcedureTypes.Banning, Team2);
                case "bw":
                    return (MatchProcedureTypes.Banning, RollWinner);
                case "bl":
                    return (MatchProcedureTypes.Banning, RollLoser);
                case "tb":
                    return (MatchProcedureTypes.TieBreaker, null);
                case "free1":
                    return (MatchProcedureTypes.FreePoint, Team1);
                case "free2":
                    return (MatchProcedureTypes.FreePoint, Team2);
                case "freew":
                    return (MatchProcedureTypes.FreePoint, RollWinner);
                case "freel":
                    return (MatchProcedureTypes.FreePoint, RollLoser);
                default:
                    return (MatchProcedureTypes.SettingUp, null);
            }
        }
    }
}
