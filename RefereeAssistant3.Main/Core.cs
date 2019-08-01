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
            //LoadTournaments();
            //if (!Tournaments.Any())
            //{
                CreateExampleTournament();
                LoadTournaments();
            //}
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
            var exampleConfiguration = new TournamentConfiguration { TournamentName = "osu! Asia Oceania Tournament" };
            var exampleStages = new List<TournamentStageConfiguration>
            {
                //new TournamentStageConfiguration
                //{
                //    TournamentStageName = "Qualifiers",
                //    Mappool = new Mappool
                //    {
                //        NoMod = new List<Map> { new Map(776951, "NM1"), new Map(100784, "NM2"), new Map(1467593, "NM3") },
                //        Hidden = new List<Map> { new Map(1070437, "HD1"), new Map(975036, "HD2") },
                //        HardRock = new List<Map> { new Map(390889, "HR1"), new Map(1490853, "HR2") },
                //        DoubleTime = new List<Map> { new Map(42352, "DT1"), new Map(125325, "DT2") },
                //        FreeMod = new List<Map> { new Map(441472, "FM1"), new Map(1827324, "FM2") }
                //    },
                //    MatchProceedings = "BL BW PW PL PW PL PW PL PW PL TB".Split(' ').ToList(),
                //    ScoreRequiredToWin = 5,
                //    DoFailedScoresCount = true,
                //    RoomSettings = new MpRoomSettings
                //    {
                //        RoomName = "osu! Example Tournament: (TEAM1) vs (TEAM2)",
                //        ScoreMode = ScoreMode.ScoreV2,
                //        TeamMode = TeamMode.HeadToHead
                //    }
                //},
                new TournamentStageConfiguration
                {
                    TournamentStageName = "Group Stage",
                    Mappool = new Mappool
                    {
                        NoMod = new List<Map>
                        {
                            new Map(1761079, "NM1"),
                            new Map(1083206, "NM2"),
                            new Map(1627723, "NM3"),
                            new Map(2115312, "NM4")
                        },
                        Hidden = new List<Map>
                        {
                            new Map(2006992, "HD1"),
                            new Map(98163, "HD2")
                        },
                        HardRock = new List<Map>
                        {
                            new Map(1280901, "HR1"),
                            new Map(1957780, "HR2")
                        },
                        DoubleTime = new List<Map>
                        {
                            new Map(1725871, "DT1"),
                            new Map(196686, "DT2")
                        },
                        FreeMod = new List<Map>
                        {
                            new Map(1104333, "FM1"),
                            new Map(188061, "FM2")
                        },
                        Other = new List<Map>
                        {
                            new Map(1759782, "TB")
                        }
                    },
                    MatchProceedings = "Roll BL BW PW PL PW PL PW PL PW PL TB".Split(' ').ToList(),
                    ScoreRequiredToWin = 5,
                    DoFailedScoresCount = true,
                    RoomSettings = new MpRoomSettings
                    {
                        RoomName = "o!AOT: (TEAM1) vs (TEAM2)",
                        ScoreMode = ScoreMode.ScoreV2,
                        TeamMode = TeamMode.TeamVs
                    }
                },
                //new TournamentStageConfiguration
                //{
                //    TournamentStageName = "Grand Finals",
                //    Mappool = new Mappool
                //    {
                //        NoMod = new List<Map> { new Map(776951, "NM1"), new Map(100784, "NM2"), new Map(1467593, "NM3") },
                //        Hidden = new List<Map> { new Map(1070437, "HD1"), new Map(975036, "HD2") },
                //        HardRock = new List<Map> { new Map(390889, "HR1"), new Map(1490853, "HR2") },
                //        DoubleTime = new List<Map> { new Map(42352, "DT1"), new Map(125325, "DT2") },
                //        FreeMod = new List<Map> { new Map(441472, "FM1"), new Map(1827324, "FM2") }
                //    },
                //    MatchProceedings = "Free1 Warm1 Warm2 Roll BL BW PW PL PW PL BL BW PW PL B1 PW PL PW PL PW TB".Split(' ').ToList(),
                //    ScoreRequiredToWin = 7,
                //    RoomSettings = new MpRoomSettings
                //    {
                //        RoomName = "o!ExT Grand Finals: (TEAM1) vs (TEAM2)",
                //        ScoreMode = ScoreMode.ScoreV2,
                //        TeamMode = TeamMode.TeamVs,
                //    },
                //    DoFailedScoresCount = true
                //},
            };
            var exampleTeams = JsonConvert.DeserializeObject<List<TeamStorage>>(@"[
  {
    ""TeamName"": ""黃建智"",
    ""Members"": [
      {
        ""PlayerId"": 959763
      },
      {
        ""PlayerId"": 5155973
      },
      {
        ""PlayerId"": 3163649
      },
      {
        ""PlayerId"": 1860489
      }
    ]
  },
  {
    ""TeamName"": ""reyuza ganteng"",
    ""Members"": [
      {
        ""PlayerId"": 4750008
      },
      {
        ""PlayerId"": 2454767
      },
      {
        ""PlayerId"": 1987591
      },
      {
        ""PlayerId"": 2312106
      }
    ]
  },
  {
    ""TeamName"": ""HoChRaPi"",
    ""Members"": [
      {
        ""PlayerId"": 7785655
      },
      {
        ""PlayerId"": 7274010
      },
      {
        ""PlayerId"": 4578623
      },
      {
        ""PlayerId"": 4945688
      }
    ]
  },
  {
    ""TeamName"": ""Sutiire"",
    ""Members"": [
      {
        ""PlayerId"": 6673790
      },
      {
        ""PlayerId"": 5449433
      },
      {
        ""PlayerId"": 13165922
      }
    ]
  },
  {
    ""TeamName"": ""any ideas"",
    ""Members"": [
      {
        ""PlayerId"": 3426414
      },
      {
        ""PlayerId"": 3099689
      },
      {
        ""PlayerId"": 4659319
      },
      {
        ""PlayerId"": 2425779
      }
    ]
  },
  {
    ""TeamName"": ""FYP 228922"",
    ""Members"": [
      {
        ""PlayerId"": 1787171
      },
      {
        ""PlayerId"": 6716499
      },
      {
        ""PlayerId"": 3995630
      },
      {
        ""PlayerId"": 4845266
      }
    ]
  },
  {
    ""TeamName"": ""matsuyoku-rotsoforce"",
    ""Members"": [
      {
        ""PlayerId"": 5296112
      },
      {
        ""PlayerId"": 5286213
      },
      {
        ""PlayerId"": 7332068
      },
      {
        ""PlayerId"": 7435670
      }
    ]
  },
  {
    ""TeamName"": ""No title"",
    ""Members"": [
      {
        ""PlayerId"": 11469447
      },
      {
        ""PlayerId"": 909745
      },
      {
        ""PlayerId"": 4569302
      },
      {
        ""PlayerId"": 10764012
      }
    ]
  },
  {
    ""TeamName"": ""모모니나"",
    ""Members"": [
      {
        ""PlayerId"": 1206417
      },
      {
        ""PlayerId"": 2190336
      }
    ]
  },
  {
    ""TeamName"": ""No Tryhard"",
    ""Members"": [
      {
        ""PlayerId"": 4003979
      },
      {
        ""PlayerId"": 2199427
      },
      {
        ""PlayerId"": 5718989
      },
      {
        ""PlayerId"": 7246165
      }
    ]
  },
  {
    ""TeamName"": ""an unlikely alliance"",
    ""Members"": [
      {
        ""PlayerId"": 4601372
      },
      {
        ""PlayerId"": 9498319
      },
      {
        ""PlayerId"": 13211236
      }
    ]
  },
  {
    ""TeamName"": ""Simple"",
    ""Members"": [
      {
        ""PlayerId"": 11776882
      },
      {
        ""PlayerId"": 9419541
      },
      {
        ""PlayerId"": 8621203
      },
      {
        ""PlayerId"": 9737589
      }
    ]
  },
  {
    ""TeamName"": ""panda is not justice"",
    ""Members"": [
      {
        ""PlayerId"": 5791401
      },
      {
        ""PlayerId"": 2190156
      },
      {
        ""PlayerId"": 1646397
      },
      {
        ""PlayerId"": 629717
      }
    ]
  },
  {
    ""TeamName"": ""Bin Boys"",
    ""Members"": [
      {
        ""PlayerId"": 5751823
      },
      {
        ""PlayerId"": 7341183
      },
      {
        ""PlayerId"": 11652827
      },
      {
        ""PlayerId"": 4166621
      }
    ]
  },
  {
    ""TeamName"": ""Nov 14th"",
    ""Members"": [
      {
        ""PlayerId"": 4585186
      },
      {
        ""PlayerId"": 6537257
      },
      {
        ""PlayerId"": 7326548
      },
      {
        ""PlayerId"": 9146185
      }
    ]
  },
  {
    ""TeamName"": ""Neo Fantasy Online"",
    ""Members"": [
      {
        ""PlayerId"": 7904667
      },
      {
        ""PlayerId"": 5875419
      },
      {
        ""PlayerId"": 3677251
      }
    ]
  },
  {
    ""TeamName"": ""babote"",
    ""Members"": [
      {
        ""PlayerId"": 1603923
      },
      {
        ""PlayerId"": 1883865
      },
      {
        ""PlayerId"": 1021944
      },
      {
        ""PlayerId"": 1629553
      }
    ]
  },
  {
    ""TeamName"": ""The Big Dilfs"",
    ""Members"": [
      {
        ""PlayerId"": 7477458
      },
      {
        ""PlayerId"": 4012086
      },
      {
        ""PlayerId"": 3698691
      },
      {
        ""PlayerId"": 3641404
      }
    ]
  },
  {
    ""TeamName"": ""12 jobs a month"",
    ""Members"": [
      {
        ""PlayerId"": 5754859
      },
      {
        ""PlayerId"": 7109508
      },
      {
        ""PlayerId"": 5052238
      },
      {
        ""PlayerId"": 5978907
      }
    ]
  },
  {
    ""TeamName"": ""Team Smead"",
    ""Members"": [
      {
        ""PlayerId"": 3751116
      },
      {
        ""PlayerId"": 6934358
      },
      {
        ""PlayerId"": 1266102
      },
      {
        ""PlayerId"": 2419478
      }
    ]
  },
  {
    ""TeamName"": ""Operation Bathroom"",
    ""Members"": [
      {
        ""PlayerId"": 3997580
      },
      {
        ""PlayerId"": 4999984
      },
      {
        ""PlayerId"": 6916774
      },
      {
        ""PlayerId"": 2118945
      }
    ]
  },
  {
    ""TeamName"": ""osu attack on titan"",
    ""Members"": [
      {
        ""PlayerId"": 3717749
      },
      {
        ""PlayerId"": 1868745
      },
      {
        ""PlayerId"": 9373724
      },
      {
        ""PlayerId"": 7910282
      }
    ]
  },
  {
    ""TeamName"": ""wat"",
    ""Members"": [
      {
        ""PlayerId"": 4555814
      },
      {
        ""PlayerId"": 3432672
      },
      {
        ""PlayerId"": 2719307
      }
    ]
  },
  {
    ""TeamName"": ""tim jago strim"",
    ""Members"": [
      {
        ""PlayerId"": 11367222
      },
      {
        ""PlayerId"": 5968633
      },
      {
        ""PlayerId"": 4655584
      },
      {
        ""PlayerId"": 2715574
      }
    ]
  },
  {
    ""TeamName"": ""과로사"",
    ""Members"": [
      {
        ""PlayerId"": 6076529
      },
      {
        ""PlayerId"": 4133758
      },
      {
        ""PlayerId"": 7892320
      },
      {
        ""PlayerId"": 7302146
      }
    ]
  },
  {
    ""TeamName"": ""okguysweneedaname"",
    ""Members"": [
      {
        ""PlayerId"": 5447609
      },
      {
        ""PlayerId"": 3517706
      },
      {
        ""PlayerId"": 6437601
      },
      {
        ""PlayerId"": 195946
      }
    ]
  },
  {
    ""TeamName"": ""艾莉皮肤店"",
    ""Members"": [
      {
        ""PlayerId"": 694480
      },
      {
        ""PlayerId"": 4764671
      },
      {
        ""PlayerId"": 6649605
      },
      {
        ""PlayerId"": 3045895
      }
    ]
  },
  {
    ""TeamName"": ""Hot Loli(s)"",
    ""Members"": [
      {
        ""PlayerId"": 9606647
      },
      {
        ""PlayerId"": 8660120
      },
      {
        ""PlayerId"": 2373484
      },
      {
        ""PlayerId"": 3484548
      }
    ]
  },
  {
    ""TeamName"": ""進撃のバブルティー"",
    ""Members"": [
      {
        ""PlayerId"": 3068044
      },
      {
        ""PlayerId"": 3345902
      },
      {
        ""PlayerId"": 832084
      },
      {
        ""PlayerId"": 3478883
      }
    ]
  },
  {
    ""TeamName"": ""bus girls"",
    ""Members"": [
      {
        ""PlayerId"": 8641416
      },
      {
        ""PlayerId"": 2523703
      },
      {
        ""PlayerId"": 7863657
      },
      {
        ""PlayerId"": 4293459
      }
    ]
  },
  {
    ""TeamName"": ""Best Friends"",
    ""Members"": [
      {
        ""PlayerId"": 6222880
      },
      {
        ""PlayerId"": 5783393
      },
      {
        ""PlayerId"": 3540294
      },
      {
        ""PlayerId"": 6862265
      }
    ]
  },
  {
    ""TeamName"": ""Chicken Biriyani"",
    ""Members"": [
      {
        ""PlayerId"": 1788022
      },
      {
        ""PlayerId"": 6636960
      },
      {
        ""PlayerId"": 2003720
      }
    ]
  },
  {
    ""TeamName"": ""Test1"",
    ""Members"": [
      {
        ""PlayerId"": 4185566
      }
    ]
  },
  {
    ""TeamName"": ""Test2"",
    ""Members"": [
      {
        ""PlayerId"": 1372608
      }
    ]
  }
]");
            new Tournament(exampleConfiguration, exampleStages, exampleTeams).Save();
        }
    }
}
