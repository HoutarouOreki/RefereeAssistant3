using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Tournaments;
using System;

namespace RefereeAssistant3.Visual.Screens.TournamentConf
{
    /// <summary>
    /// A screen responsible for letting the user
    /// create and edit tournament configurations.
    /// </summary>
    public class TournamentConfigurationScreen : ScreenWithSelection<Tournament>
    {
        public TournamentConfigurationScreen(Core core) : base(tc => tc.Configuration.TournamentName, core.Tournaments)
        {
        }
    }
}
