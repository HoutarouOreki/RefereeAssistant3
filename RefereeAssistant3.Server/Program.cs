using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace RefereeAssistant3.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Settings.LoadSettings();
            if (Settings.ConnectionString == null)
            {
                Console.WriteLine($"Connection string is not specified ({Settings.SettingsFile})");
                Console.ReadKey(true);
                return;
            }
            CreateWebHostBuilder(args).UseUrls("http://localhost:5000", "https://localhost:5001").Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
