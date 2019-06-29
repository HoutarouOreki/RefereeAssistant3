using Newtonsoft.Json;
using osu.Framework;
using RefereeAssistant3.Main;
using RefereeAssistant3.Visual;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3
{
    public class Program
    {
        private static readonly DirectoryInfo dir = Utilities.GetBaseDirectory();
        private static readonly DirectoryInfo tournaments_directory = new DirectoryInfo($"{dir}/tournaments");

        public static int Main()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            //var cwd = Environment.CurrentDirectory;
            using (var host = Host.GetSuitableHost(@"RefereeAssistant3", true))
            {
                //host.ExceptionThrown += HandleException;

                Directory.SetCurrentDirectory(dir.FullName);
                if (!dir.Exists || !tournaments_directory.Exists || tournaments_directory.GetDirectories().Length == 0)
                    CreateExampleTournament();

                var tournaments = GenerateTournaments();

                if (!tournaments.Any())
                {
                    CreateExampleTournament();
                    tournaments = GenerateTournaments();
                }

                var core = new Core(tournaments);

                //switch (args.FirstOrDefault() ?? string.Empty)
                //{
                //    default:
                //        var fullOrBasic = 0;
                //        Console.WriteLine("1 for visual, 2 for basic");
                //        while (fullOrBasic < 1 || fullOrBasic > 2)
                //            int.TryParse(Console.ReadKey().KeyChar.ToString(), out fullOrBasic);
                //        Console.WriteLine($"Launching {(fullOrBasic == 1 ? "visual" : "basic")} mode...");
                //        if (fullOrBasic == 1)
                host.Run(new RefereeAssistant3Visual(core));
                //        else
                //            break;
                //        break;
                //}

                return 0;
            }
        }

        private static IEnumerable<Tournament> GenerateTournaments()
        {
            var tournamentTasks = new List<Task<Tournament>>();

            foreach (var tournamentDirectory in tournaments_directory.GetDirectories())
            {
                var confFile = new FileInfo($"{tournamentDirectory}/configuration.json");
                var stagesFile = new FileInfo($"{tournamentDirectory}/stages.json");
                var teamsFile = new FileInfo($"{tournamentDirectory}/teams.json");
                if (!confFile.Exists || !stagesFile.Exists || !teamsFile.Exists)
                    continue;
                tournamentTasks.Add(CreateTournament(confFile.FullName, stagesFile.FullName, teamsFile.FullName));
            }

            Task.WaitAll(tournamentTasks.ToArray());

            return tournamentTasks.Select(tt => tt.Result);
        }

        private static async Task<Tournament> CreateTournament(string confFile, string stagesFile, string teamsFile)
        {
            var teamsFileTask = File.ReadAllTextAsync(teamsFile);
            var confFileTask = File.ReadAllTextAsync(confFile);
            var stagesFileTask = File.ReadAllTextAsync(stagesFile);

            await Task.WhenAll(teamsFileTask, confFileTask, stagesFileTask);

            var teams = JsonConvert.DeserializeObject<List<Team>>(teamsFileTask.Result);
            var stages = JsonConvert.DeserializeObject<List<TournamentStage>>(stagesFileTask.Result);
            var configuration = JsonConvert.DeserializeObject<TournamentConfiguration>(confFileTask.Result);
            return new Tournament(configuration, stages, teams);
        }

        private static void CreateExampleTournament()
        {
            dir.Create();
            tournaments_directory.Create();
            var exampleTournament = new DirectoryInfo($"{tournaments_directory}/Example Tournament");
            exampleTournament.Create();

            var exampleConfiguration = new TournamentConfiguration
            {
                Name = "Example Tournament",
                DoFailedScoresCount = false
            };
            File.WriteAllText($"{exampleTournament}/configuration.json", JsonConvert.SerializeObject(exampleConfiguration));

            var exampleStages = new List<TournamentStage>
            {
                new TournamentStage
                {
                    TournamentStageName = "Group Stage",
                    Mappool = new Mappool
                    {
                        DoubleTime = new List<Map> { new Map(42352), new Map(125325) },
                        NoMod = new List<Map> { new Map(776951), new Map(100784), new Map(1467593) },
                        Hidden = new List<Map> { new Map(1070437), new Map(975036) },
                        HardRock = new List<Map> { new Map(390889), new Map(1490853) },
                        FreeMod = new List<Map> { new Map(441472), new Map(1827324) }
                    },
                    MatchProceedings = "Roll BL BW PW PL PW PL PW PL PW PL TB".Split(' ').ToList(),
                    RoomName = "osu! Example Tournament: (TEAM1) vs (TEAM2)",
                    ScoreRequiredToWin = 5
                },
                new TournamentStage
                {
                    TournamentStageName = "Grand Finals",
                    Mappool = new Mappool
                    {
                        DoubleTime = new List<Map> { new Map(42352), new Map(125325) },
                        NoMod = new List<Map> { new Map(776951), new Map(100784), new Map(1467593) },
                        Hidden = new List<Map> { new Map(1070437), new Map(975036) },
                        HardRock = new List<Map> { new Map(390889), new Map(1490853) },
                        FreeMod = new List<Map> { new Map(441472), new Map(1827324) }
                    },
                    MatchProceedings = "Free1 Warm1 Warm2 Roll BL BW PW PL PW PL BL BW PW PL B1 PW PL PW PL PW TB".Split(' ').ToList(),
                    RoomName = "o!ExT Grand Finals: (TEAM1) vs (TEAM2)",
                    ScoreRequiredToWin = 7
                },
            };
            File.WriteAllText($"{exampleTournament}/stages.json", JsonConvert.SerializeObject(exampleStages));
            var exampleTeams = new List<Team>
                    {
                        new Team("Animals", new List<Player>
                        { new Player(4185566), new Player(1372608), new Player(7866701) }),
                        new Team("Joestars", new List<Player>
                        { new Player(9299739), new Player(7366346), new Player(11351311) }),
                        new Team("Cars", new List<Player>
                        { new Player(8654962), new Player(772248) })
                    };
            File.WriteAllText($"{exampleTournament}/teams.json", JsonConvert.SerializeObject(exampleTeams));
        }
    }
}
