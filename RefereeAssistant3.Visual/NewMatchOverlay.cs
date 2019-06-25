using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;
using System;

namespace RefereeAssistant3.Visual
{
    public class NewMatchOverlay : OverlayContainer
    {
        private readonly Core core;

        private readonly Dropdown<Team> team1Dropdown;
        private readonly Dropdown<Team> team2Dropdown;

        public NewMatchOverlay(Core core)
        {
            this.core = core;
            Children = new Drawable[]
            {
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(Style.SPACING),
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = new Vector2(Style.SPACING),
                        Children = new Drawable[]
                        {
                            team1Dropdown = new BasicDropdown<Team>
                            {
                                Width = Style.COMPONENTS_WIDTH,
                                //Height = COMPONENTS_HEIGHT,
                            },
                            team2Dropdown = new BasicDropdown<Team>
                            {
                                Width = Style.COMPONENTS_WIDTH,
                                //Height = COMPONENTS_HEIGHT,
                            },
                            new Button
                            {
                                Text = "Add match",
                                Action = AddNewMatch,
                                Width = Style.COMPONENTS_WIDTH,
                                Height = Style.COMPONENTS_HEIGHT,
                                BackgroundColour = FrameworkColour.Green,
                            }
                        }
                    }
                }
            };
            team1Dropdown.Items = team2Dropdown.Items = core.Teams;
        }

        protected override void PopIn() => Show();
        protected override void PopOut() => Hide();

        private void AddNewMatch()
        {
            var team1 = team1Dropdown.Current.Value;
            var team2 = team2Dropdown.Current.Value;
            if (team1 == null || team2 == null || team1 == team2)
                return;
            var match = new Match(team1, team2, new Mappool(), TournamentStage.Qualifiers);
            core.AddNewMatch(match);
        }
    }
}
