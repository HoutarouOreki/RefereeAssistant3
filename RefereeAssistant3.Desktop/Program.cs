using Newtonsoft.Json;
using osu.Framework;
using RefereeAssistant3.Main;
using RefereeAssistant3.Visual;
using System.IO;

namespace RefereeAssistant3
{
    public class Program
    {
        private static readonly DirectoryInfo dir = Utilities.GetBaseDirectory();

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
                dir.CreateSubdirectory("cache/players");
                dir.CreateSubdirectory("cache/maps");

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
