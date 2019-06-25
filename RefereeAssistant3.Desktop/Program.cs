using osu.Framework;
using RefereeAssistant3.Main;
using RefereeAssistant3.Visual;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RefereeAssistant3
{
    public class Program
    {
        public static int Main(string[] args)
        {
            //var cwd = Environment.CurrentDirectory;
            using (var host = Host.GetSuitableHost(@"Referee Assistant 3", true, true))
            {
                //host.ExceptionThrown += HandleException;

                var dir = Utilities.GetBaseDirectory();
                if (!dir.Exists || !File.Exists($"{dir}/teams.txt"))
                {
                    dir.Create();
                    File.WriteAllText($"{dir}/teams.txt", "teamName:captain,player2,player3");
                }
                if (File.ReadAllLines($"{dir}/teams.txt").Length < 2)
                {
                    Console.WriteLine($"Add teams to {dir}/teams.txt according to this syntax:");
                    Console.WriteLine("Team:captain,player2,player3");
                    Console.WriteLine("and then run again.");
                    Console.WriteLine("Press 1 to open the file. Or close the program.");
                    var res = Console.ReadKey(true);
                    if (res.Key == ConsoleKey.D1)
                        host.OpenFileExternally($"{dir}/teams.txt");
                    Environment.Exit(0);
                }

                var teamsText = File.ReadAllLines($"{dir}/teams.txt");
                var teams = new List<Team>(teamsText.Length);
                foreach (var teamLine in teamsText.Where(t => t.Contains(':') && t.Length > 4))
                {
                    var temp = teamLine.Split(':');
                    var teamName = temp[0];
                    var memberNames = temp[1].Split(',');
                    var team = new Team(teamName, memberNames.Select(name => new Player(name)));
                    teams.Add(team);
                }

                var core = new MainLoop(teams);

                switch (args.FirstOrDefault() ?? string.Empty)
                {
                    default:
                        var fullOrBasic = 0;
                        Console.WriteLine("1 for visual, 2 for basic");
                        while (fullOrBasic < 1 || fullOrBasic > 2)
                            int.TryParse(Console.ReadKey().KeyChar.ToString(), out fullOrBasic);
                        Console.WriteLine($"Launching {(fullOrBasic == 1 ? "visual" : "basic")} mode...");
                        if (fullOrBasic == 1)
                            host.Run(new RefereeAssistant3Visual(core));
                        else
                            break;
                        break;
                }

                return 0;
            }
        }
    }
}
