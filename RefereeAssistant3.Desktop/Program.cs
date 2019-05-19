using osu.Framework;
using RefereeAssistant3.Visual;
using System;
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

                switch (args.FirstOrDefault() ?? string.Empty)
                {
                    default:
                        var fullOrBasic = 0;
                        Console.WriteLine("1 for visual, 2 for basic");
                        while (fullOrBasic < 1 || fullOrBasic > 2)
                            int.TryParse(Console.ReadKey().KeyChar.ToString(), out fullOrBasic);
                        Console.WriteLine($"Launching {(fullOrBasic == 1 ? "visual" : "basic")} mode...");
                        if (fullOrBasic == 1)
                            host.Run(new RefereeAssistant3Visual());
                        else
                            break;
                        break;
                }

                return 0;
            }
        }
    }
}
