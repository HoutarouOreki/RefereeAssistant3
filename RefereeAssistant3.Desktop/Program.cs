using osu.Framework;
using RefereeAssistant3.Main;
using RefereeAssistant3.Visual;
using System.Collections.Generic;
using System.IO;

namespace RefereeAssistant3
{
    public class Program
    {
        public static int Main()
        {
            //var cwd = Environment.CurrentDirectory;
            using (var host = Host.GetSuitableHost(@"Referee Assistant 3", true, true))
            {
                //host.ExceptionThrown += HandleException;

                var dir = Utilities.GetBaseDirectory();
                var tournamentsDirectory = new DirectoryInfo($"{dir}/tournaments");
                if (!dir.Exists || !tournamentsDirectory.Exists || tournamentsDirectory.GetDirectories().Length == 0)
                {
                    dir.Create();
                    tournamentsDirectory.Create();
                    var exampleTournament = new DirectoryInfo($"{tournamentsDirectory}/Example Tournament");
                    exampleTournament.Create();
                    File.WriteAllLines($"{exampleTournament}/configuration.txt", new[]
                    {
                        "Example Tournament",
                        "no"
                    });
                    File.WriteAllLines($"{exampleTournament}/stages.txt", new[]
                    {
                        "Group Stage|||o!AOT: (TEAM1) vs (TEAM2)|||Roll BL BW PW PL PW PL PW PL PW PL TB|||5",
                        "___",
                        "NM1|||42352|||Lady Gaga - Bad Romance [Extra]|||NM",
                        "HD1|||1070437|||Dave Rodgers - Deja Vu [Multi Track Drifting]|||HD",
                        "FM1|||432783249|||Kansas - Carry on Wayward [Supernatural]|||FM",
                        "###",
                        "Grand Finals|||o!AOT Grand Finals: (TEAM1) vs (TEAM2)|||Free1 Warm1 Warm2 Roll BL BW PW PL PW PL BL BW PW PL B1 PW PL PW PL PW TB|||7",
                        "___",
                        "NM1|||42352|||Lady Gaga - Bad Romance [Extra]|||NM",
                        "HD1|||1070437|||Dave Rodgers - Deja Vu [Multi Track Drifting]|||HD",
                        "FM1|||432783249|||Kansas - Carry on Wayward [Supernatural]|||FM"
                    });
                    File.WriteAllLines($"{exampleTournament}/teams.txt", new[]
                    {
                        "teamName1:Houtarou Oreki,nya10",
                        "teamName2:Kujo Qtaro,Fujiwara Takumi"
                    });
                }

                var tournaments = new List<Tournament>();

                foreach (var tournamentDirectory in tournamentsDirectory.GetDirectories())
                {
                    var confFile = new FileInfo($"{tournamentDirectory}/configuration.txt");
                    var stagesFile = new FileInfo($"{tournamentDirectory}/stages.txt");
                    var teamsFile = new FileInfo($"{tournamentDirectory}/teams.txt");
                    if (!confFile.Exists || !stagesFile.Exists || !teamsFile.Exists)
                        continue;
                    tournaments.Add(new Tournament(File.ReadAllText(confFile.FullName), File.ReadAllText(stagesFile.FullName), File.ReadAllText(teamsFile.FullName)));
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
    }
}
