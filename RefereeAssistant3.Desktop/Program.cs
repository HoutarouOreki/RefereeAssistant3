using Newtonsoft.Json;
using osu.Framework;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Utilities;
using RefereeAssistant3.Visual;
using System.IO;

namespace RefereeAssistant3
{
    public class Program
    {
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

                PathUtilities.RootProgramDirectory.Create();
                Directory.SetCurrentDirectory(PathUtilities.RootProgramDirectory.FullName);
                PathUtilities.MapsCacheDirectory.Create();
                PathUtilities.PlayersCacheDirectory.Create();
                PathUtilities.SavedMatchesDirectory.Create();
                PathUtilities.SavedHeadToHeadMatchesDirectory.Create();
                PathUtilities.SavedTeamVsMatchesDirectory.Create();
                PathUtilities.TournamentsDirectory.Create();

                var core = new Core();

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
