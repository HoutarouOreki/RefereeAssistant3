using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;
using System.Collections.Generic;

namespace RefereeAssistant3.Visual
{
    public class RefereeAssistant3Visual : Game
    {
        private readonly List<Match> matches = new List<Match>();
        private Dropdown<Team> team1Dropdown;
        private Dropdown<Team> team2Dropdown;
        private List<Team> teams;
        public const float COMPONENTS_WIDTH = 100;
        public const float COMPONENTS_HEIGHT = 30;
        public const float SPACING = 24;

        public RefereeAssistant3Visual(List<Team> teams) => this.teams = teams;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(SPACING),
                    Children = new Drawable[]
                    {
                        team1Dropdown = new BasicDropdown<Team>
                        {
                            Width = COMPONENTS_WIDTH,
                            //Height = COMPONENTS_HEIGHT,
                        },
                        team2Dropdown = new BasicDropdown<Team>
                        {
                            Width = COMPONENTS_WIDTH,
                            //Height = COMPONENTS_HEIGHT,
                        },
                        new Button
                        {
                            Text = "Add match",
                            Action = AddNewMatch,
                            Width = COMPONENTS_WIDTH,
                            Height = COMPONENTS_HEIGHT,
                            BackgroundColour = FrameworkColour.Green,
                        }
                    }
                }
            };
            team1Dropdown.Items = team2Dropdown.Items = teams;
        }

        private void AddNewMatch()
        {
            var team1 = team1Dropdown.Current.Value;
            var team2 = team2Dropdown.Current.Value;
            if (team1 == null || team2 == null || team1 == team2)
                return;
            var match = new Match(team1, team2, new Mappool(), TournamentStage.Qualifiers);
            matches.Add(match);
            // update match list
        }
    }
}
