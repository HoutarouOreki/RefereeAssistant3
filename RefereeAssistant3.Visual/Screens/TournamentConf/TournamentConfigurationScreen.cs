using osu.Framework.Bindables;
using osu.Framework.Screens;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Tournaments;
using RefereeAssistant3.Visual.UI;

namespace RefereeAssistant3.Visual.Screens.TournamentConf
{
    /// <summary>
    /// A screen responsible for letting the user
    /// create and edit tournament configurations.
    /// </summary>
    public class TournamentConfigurationScreen : ScreenWithSelection<Tournament>
    {
        private readonly RA3Button configureStagesButton;
        private readonly Core core;

        public TournamentConfigurationScreen(Core core) : base(tc => tc.Configuration.TournamentName, core.Tournaments, "Tournament configuration")
        {
            Content.Add(configureStagesButton = new RA3Button
            {
                Size = Style.COMPONENTS_SIZE,
                Text = "Configure stages"
            });
            this.core = core;
        }

        protected override void OnCurrentChanged(ValueChangedEvent<Tournament> currentChange)
        {
            if (currentChange.NewValue == null)
                configureStagesButton.Action = null;
            else
                configureStagesButton.Action = () => this.Push(new StageConfigurationScreen(core, Current.Value));
            base.OnCurrentChanged(currentChange);
        }
    }
}
