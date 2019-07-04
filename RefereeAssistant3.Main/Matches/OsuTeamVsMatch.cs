using RefereeAssistant3.Main.Online.APIModels;
using RefereeAssistant3.Main.Tournaments;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.Matches
{
    public class OsuTeamVsMatch : OsuMatch<Team>
    {
        public Team Team1 => Participants[0];

        public Team Team2 => Participants[1];

        public Team RollLoser => RollWinner == null ? null :
            Team1 == RollWinner ? Team2 : Team1;

        public Team Winner => Scores[Team1] == TournamentStage.ScoreRequiredToWin ? Team1 :
            Scores[Team2] == TournamentStage.ScoreRequiredToWin ? Team2 : null;

        public override string WinnerName => Winner.Name;

        public string RoomName => TournamentStage.RoomName.Replace("TEAM1", Team1.TeamName).Replace("TEAM2", Team2.TeamName);

        public OsuTeamVsMatch(Team team1, Team team2, Tournament tournament, TournamentStage tournamentStage) : base(tournament, tournamentStage)
        {
            Participants.Add(team1);
            Participants.Add(team2);
            Scores.Add(Team1, 0);
            Scores.Add(Team2, 0);
            GenerateMatchProcedures();
        }

        public OsuTeamVsMatch(APIMatch apiMatch, Team team1, Team team2, Tournament tournament, TournamentStage tournamentStage) : base(apiMatch, tournament, tournamentStage)
        {
            Participants.Add(team1);
            Scores[Team1] = apiMatch.Participants[0].Score ?? 0;
            Participants.Add(team2);
            Scores[Team2] = apiMatch.Participants[1].Score ?? 1;
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
            foreach (var mapResult in apiMatch.MapResults)
            {
                MapResults.Add(new Map(mapResult.DifficultyId), new Dictionary<APIPlayer, int>(mapResult.PlayerScores.Select(kv => new KeyValuePair<APIPlayer, int>(new APIPlayer(kv.Key), kv.Value))));
            }
            GenerateMatchProcedures();
        }

        public override Player GetPlayer(string username) => Team1.Members.Concat(Team2.Members).FirstOrDefault(p => p.IRCUsername == username || p.Username == username);

        protected override OsuMatchSnapshot CreateSnapshot() => new OsuMatchSnapshot
        {
            MapResults = new List<MapResult>(MapResults.Select(mapPlayerScore => new MapResult
            {
                DifficultyId = mapPlayerScore.Key.DifficultyId,
                PlayerScores = new Dictionary<int, int>(mapPlayerScore.Value.Select(playerScore => new KeyValuePair<int, int>(playerScore.Key.PlayerId, playerScore.Value)))
            })),
            Name = CurrentProcedure.Name,
            Participants = Participants.Select(p => new APIParticipant(p, Scores[p])).ToList(),
            ProcedureIndex = CurrentProcedureIndex,
            RollWinnersName = RollWinner?.Name,
            SelectedMap = SelectedMap?.DifficultyId,
            SelectedWinner = SelectedWinner == null ? null : new APIParticipant(SelectedWinner)
        };

        protected override void SetStateFromSnapshot(OsuMatchSnapshot lastSnapshot)
        {
            CurrentProcedureIndex = lastSnapshot.ProcedureIndex;
            Scores[Team1] = lastSnapshot.Participants[0].Score ?? 0;
            Scores[Team2] = lastSnapshot.Participants[1].Score ?? 0;
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
