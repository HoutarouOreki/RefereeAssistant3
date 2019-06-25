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
        private TextBox player1TextBox;
        private TextBox player2TextBox;
        public const float COMPONENTS_WIDTH = 100;
        public const float COMPONENTS_HEIGHT = 30;
        public const float SPACING = 24;

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
                        player1TextBox = new BasicTextBox
                        {
                            PlaceholderText = "Player 1 username",
                            Width = COMPONENTS_WIDTH,
                            Height = COMPONENTS_HEIGHT,
                        },
                        player2TextBox = new BasicTextBox
                        {
                            PlaceholderText = "Player 2 username",
                            Width = COMPONENTS_WIDTH,
                            Height = COMPONENTS_HEIGHT,
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
        }

        private void AddNewMatch()
        {
            var player1 = new Player(player1TextBox.Text);
            var player2 = new Player(player2TextBox.Text);
            var match = new Match(new[] { player1, player2 }, new Mappool(), TournamentStage.Qualifiers);
            matches.Add(match);
        }
    }
}
