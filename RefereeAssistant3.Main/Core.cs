using Newtonsoft.Json;
using osu.Framework.Bindables;
using RefereeAssistant3.Main.IRC;
using RefereeAssistant3.Main.Matches;
using RefereeAssistant3.Main.Online.APIModels;
using RefereeAssistant3.Main.Storage;
using RefereeAssistant3.Main.Tournaments;
using RefereeAssistant3.Main.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Core
    {
        public event Action<OsuMatch> NewMatchAdded;

        public Bindable<OsuMatch> SelectedMatch = new Bindable<OsuMatch>();

        public IReadOnlyList<OsuMatch> Matches => matches;
        public List<Tournament> Tournaments { get; } = new List<Tournament>();
        public BanchoIrcManager ChatBot { get; }

        private readonly List<OsuMatch> matches = new List<OsuMatch>();

        public event Action<string> Alert;

        public Core()
        {
            LoadTournaments();
            if (!Tournaments.Any())
            {
                CreateExampleTournament();
                LoadTournaments();
            }
            MainConfig.Load();
            ChatBot = new BanchoIrcManager();
            new OsuIrcMatchParseHandler(this);
            LoadSavedMatches();
        }

        public void AddNewMatch(OsuMatch match)
        {
            match.TournamentStage.Mappool.DownloadMappoolAsync();
            match.BanchoIrc = ChatBot;
            match.PreparePlayersInfo().ContinueWith(t =>
            {
                match.GenerateMatchProcedures();
                match.Updated += () => OnMatchUpdated(match);
                matches.Add(match);
                NewMatchAdded?.Invoke(match);
            });
        }

        private void OnMatchUpdated(OsuMatch match)
        {
            var serializedMatch = JsonConvert.SerializeObject(match.GenerateAPIMatch());
            if (match is OsuTeamVsMatch teamVsMatch)
                File.WriteAllText($"{PathUtilities.SavedTeamVsMatchesDirectory}/{teamVsMatch.CreationDate:yyyy-MM-dd-HH-mm-ss}.json", serializedMatch);
        }

        private void LoadSavedMatches()
        {
            foreach (var savedMatchFile in PathUtilities.SavedTeamVsMatchesDirectory.EnumerateFiles("*.json"))
            {
                var text = File.ReadAllText(savedMatchFile.FullName);
                var apiMatch = JsonConvert.DeserializeObject<APIMatch>(text);
                var tournament = Tournaments.Find(t => t.Configuration.TournamentName == apiMatch.TournamentName);
                var tournamentStage = tournament.Stages.Find(s => s.TournamentStageName == apiMatch.TournamentStage);
                var match = new OsuTeamVsMatch(apiMatch, tournament.Teams.Find(t => t.TeamName == apiMatch.Participants[0].Name), tournament.Teams.Find(t => t.TeamName == apiMatch.Participants[1].Name), tournament, tournamentStage);
                var nameData = savedMatchFile.Name.Replace(".json", "").Split('-');
                var year = int.Parse(nameData[0]);
                var month = int.Parse(nameData[1]);
                var day = int.Parse(nameData[2]);
                var hour = int.Parse(nameData[3]);
                var minute = int.Parse(nameData[4]);
                var second = int.Parse(nameData[5]);
                match.CreationDate = new DateTime(year, month, day, hour, minute, second);
                AddNewMatch(match);
            }
            foreach (var savedMatchFile in PathUtilities.SavedHeadToHeadMatchesDirectory.EnumerateFiles("*.json"))
            {
                var text = File.ReadAllText(savedMatchFile.FullName);
                var apiMatch = JsonConvert.DeserializeObject<APIMatch>(text);
                var tournament = Tournaments.Find(t => t.Configuration.TournamentName == apiMatch.TournamentName);
                var tournamentStage = tournament.Stages.Find(s => s.TournamentStageName == apiMatch.TournamentStage);
                var match = new OsuTeamVsMatch(apiMatch, tournament.Teams.Find(t => t.TeamName == apiMatch.Participants[0].Name), tournament.Teams.Find(t => t.TeamName == apiMatch.Participants[1].Name), tournament, tournamentStage);
                var nameData = savedMatchFile.Name.Replace(".json", "").Split('-');
                var year = int.Parse(nameData[0]);
                var month = int.Parse(nameData[1]);
                var day = int.Parse(nameData[2]);
                var hour = int.Parse(nameData[3]);
                var minute = int.Parse(nameData[4]);
                var second = int.Parse(nameData[5]);
                match.CreationDate = new DateTime(year, month, day, hour, minute, second);
                AddNewMatch(match);
            }
        }

        public void PushAlert(string text) => Alert(text);

        public void LoadTournaments()
        {
            var tournamentTasks = new List<Task<Tournament>>();
            Tournaments.Clear();
            foreach (var tournamentDirectory in PathUtilities.TournamentsDirectory.GetDirectories())
            {
                var confFile = new FileInfo($"{tournamentDirectory}/configuration.json");
                var stagesFile = new FileInfo($"{tournamentDirectory}/stages.json");
                var teamsFile = new FileInfo($"{tournamentDirectory}/teams.json");
                if (!confFile.Exists || !stagesFile.Exists || !teamsFile.Exists)
                    continue;
                tournamentTasks.Add(CreateTournament(confFile.FullName, stagesFile.FullName, teamsFile.FullName));
            }

            Task.WaitAll(tournamentTasks.ToArray());

            Tournaments.AddRange(tournamentTasks.Select(tt => tt.Result));
        }

        private static async Task<Tournament> CreateTournament(string confFile, string stagesFile, string teamsFile)
        {
            var teamsFileTask = File.ReadAllTextAsync(teamsFile);
            var confFileTask = File.ReadAllTextAsync(confFile);
            var stagesFileTask = File.ReadAllTextAsync(stagesFile);

            await Task.WhenAll(teamsFileTask, confFileTask, stagesFileTask);

            var teams = JsonConvert.DeserializeObject<List<TeamStorage>>(teamsFileTask.Result);
            var stages = JsonConvert.DeserializeObject<List<TournamentStageConfiguration>>(stagesFileTask.Result);
            var configuration = JsonConvert.DeserializeObject<TournamentConfiguration>(confFileTask.Result);
            return new Tournament(configuration, stages, teams);
        }

        private static void CreateExampleTournament()
        {
            var exampleConfiguration = new TournamentConfiguration { TournamentName = "Example Tournament" };
            var exampleStages = new List<TournamentStageConfiguration>
            {
                new TournamentStageConfiguration
                {
                    TournamentStageName = "Qualifiers",
                    Mappool = new Mappool
                    {
                        NoMod = new List<Map> { new Map(776951, "NM1"), new Map(100784, "NM2"), new Map(1467593, "NM3") },
                        Hidden = new List<Map> { new Map(1070437, "HD1"), new Map(975036, "HD2") },
                        HardRock = new List<Map> { new Map(390889, "HR1"), new Map(1490853, "HR2") },
                        DoubleTime = new List<Map> { new Map(42352, "DT1"), new Map(125325, "DT2") },
                        FreeMod = new List<Map> { new Map(441472, "FM1"), new Map(1827324, "FM2") }
                    },
                    MatchProceedings = "BL BW PW PL PW PL PW PL PW PL TB".Split(' ').ToList(),
                    ScoreRequiredToWin = 5,
                    DoFailedScoresCount = true,
                    RoomSettings = new MpRoomSettings
                    {
                        RoomName = "osu! Example Tournament: (TEAM1) vs (TEAM2)",
                        ScoreMode = ScoreMode.ScoreV2,
                        TeamMode = TeamMode.TeamVs
                    }
                },
                new TournamentStageConfiguration
                {
                    TournamentStageName = "Group Stage",
                    Mappool = new Mappool
                    {
                        NoMod = new List<Map> { new Map(776951, "NM1"), new Map(100784, "NM2"), new Map(1467593, "NM3") },
                        Hidden = new List<Map> { new Map(1070437, "HD1"), new Map(975036, "HD2") },
                        HardRock = new List<Map> { new Map(390889, "HR1"), new Map(1490853, "HR2") },
                        DoubleTime = new List<Map> { new Map(42352, "DT1"), new Map(125325, "DT2") },
                        FreeMod = new List<Map> { new Map(441472, "FM1"), new Map(1827324, "FM2") }
                    },
                    MatchProceedings = "Roll BL BW PW PL PW PL PW PL PW PL TB".Split(' ').ToList(),
                    ScoreRequiredToWin = 5,
                    DoFailedScoresCount = true,
                    RoomSettings = new MpRoomSettings
                    {
                        RoomName = "osu! Example Tournament: (TEAM1) vs (TEAM2)",
                        ScoreMode = ScoreMode.ScoreV2,
                        TeamMode = TeamMode.TeamVs
                    }
                },
                new TournamentStageConfiguration
                {
                    TournamentStageName = "Grand Finals",
                    Mappool = new Mappool
                    {
                        NoMod = new List<Map> { new Map(776951, "NM1"), new Map(100784, "NM2"), new Map(1467593, "NM3") },
                        Hidden = new List<Map> { new Map(1070437, "HD1"), new Map(975036, "HD2") },
                        HardRock = new List<Map> { new Map(390889, "HR1"), new Map(1490853, "HR2") },
                        DoubleTime = new List<Map> { new Map(42352, "DT1"), new Map(125325, "DT2") },
                        FreeMod = new List<Map> { new Map(441472, "FM1"), new Map(1827324, "FM2") }
                    },
                    MatchProceedings = "Free1 Warm1 Warm2 Roll BL BW PW PL PW PL BL BW PW PL B1 PW PL PW PL PW TB".Split(' ').ToList(),
                    ScoreRequiredToWin = 7,
                    RoomSettings = new MpRoomSettings
                    {
                        RoomName = "o!ExT Grand Finals: (TEAM1) vs (TEAM2)",
                        ScoreMode = ScoreMode.ScoreV2,
                        TeamMode = TeamMode.TeamVs,
                    },
                    DoFailedScoresCount = true
                },
            };
            var exampleTeams = new List<TeamStorage>
            {
                new TeamStorage("Animals", new List<APIPlayer>
                { new APIPlayer(4185566), new APIPlayer(1372608), new APIPlayer(7866701) }),
                new TeamStorage("Joestars", new List<APIPlayer>
                { new APIPlayer(9299739), new APIPlayer(7366346), new APIPlayer(11351311) }),
                new TeamStorage("Cars", new List<APIPlayer>
                { new APIPlayer(8654962), new APIPlayer(772248) })
            };
            new Tournament(exampleConfiguration, exampleStages, exampleTeams).Save();
        }
    }
}
