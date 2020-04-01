using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Tournaments;

namespace RefereeAssistant3.Visual.Screens.TournamentConf
{
    /// <summary>
    /// A screen responsible for letting the user
    /// create and edit tournament configurations.
    /// </summary>
    public class StageConfigurationScreen : ScreenWithSelection<TournamentStageConfiguration>
    {
        public StageConfigurationScreen(Core core, Tournament tournament) : base(tc => tc.TournamentStageName, tournament.Stages, $"Tournament configuration > {tournament.Configuration.TournamentName}")
        {
        }
    }
}
