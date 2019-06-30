using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;
using RefereeAssistant3.Server.Services;

namespace RefereeAssistant3.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<MatchService>()
                .AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ConfigureLogging();

            app.UseMvc();
        }

        private void ConfigureLogging()
        {
            var logConfig = new LoggingConfiguration();
            var logFile = new FileTarget("logfile") { FileName = "nLog.txt" };
            var logConsole = new ColoredConsoleTarget("logconsole");

            logConfig.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

            LogManager.Configuration = logConfig;

            var infoLevelRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Info"),
                ForegroundColor = ConsoleOutputColor.Cyan
            };
            var warnLevelRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Warn"),
                ForegroundColor = ConsoleOutputColor.Yellow
            };
            var errorLevelRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Error"),
                ForegroundColor = ConsoleOutputColor.Red
            };
            var fatalLevelRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Fatal"),
                ForegroundColor = ConsoleOutputColor.DarkRed
            };
            logConsole.RowHighlightingRules.Add(infoLevelRule);
            logConsole.RowHighlightingRules.Add(warnLevelRule);
            logConsole.RowHighlightingRules.Add(errorLevelRule);
            logConsole.RowHighlightingRules.Add(fatalLevelRule);

            logConsole.Layout = "${longdate}|${level:uppercase=true}|${logger:shortName=true} > ${message}";
        }
    }
}
