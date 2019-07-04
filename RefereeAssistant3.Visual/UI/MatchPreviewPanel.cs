using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using RefereeAssistant3.Main.Matches;

namespace RefereeAssistant3.Visual.UI
{
    public class MatchPreviewPanel : ClickableContainer
    {
        public readonly OsuMatch Match;

        private readonly Box background;
        private readonly Box hoverOverlay;
        private readonly SpriteText stateLabel;
        private readonly SpriteText scoreText;
        private readonly SpriteText team1Label;
        private readonly SpriteText team2Label;
        private readonly Box backgroundFill;
        private readonly Container labelContainer;

        public MatchPreviewPanel(OsuMatch match)
        {
            Match = match;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.Blue
                },
                backgroundFill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.YellowGreen,
                    EdgeSmoothness = new osuTK.Vector2(1, 0)
                },
                hoverOverlay = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, Colour = Color4.Gray },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 12, Vertical = 5 },
                    Spacing = new osuTK.Vector2(4),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        labelContainer = new Container // team1 | x - y | team2
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        new Box { RelativeSizeAxes = Axes.X, Height = 1, Alpha = 0.1f },
                        stateLabel = new SpriteText { Anchor = Anchor.TopCentre, Origin = Anchor.TopCentre }
                    }
                }
            };
            match.Updated += OnMatchUpdated;
            if (Match is OsuTeamVsMatch vsMatch)
            {
                labelContainer.AddRange(new Drawable[]
                {
                    team1Label = new SpriteText
                    {
                        Text = vsMatch.Team1.TeamName,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight
                    },
                    scoreText = new SpriteText
                    { Text = "VS", Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    team2Label = new SpriteText
                    {
                        Text = vsMatch.Team2.TeamName,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft
                    }
                });
            }
            OnMatchUpdated();
        }

        private void OnMatchUpdated() => Schedule(GenerateLayout);

        private void GenerateLayout()
        {
            if (Match is OsuTeamVsMatch vsMatch)
            scoreText.Text = $"| {vsMatch.Scores[vsMatch.Team1]} - {vsMatch.Scores[vsMatch.Team2]} |";
            if (Match.CurrentProcedureType == MatchProcedureTypes.SettingUp)
                scoreText.Text = "VS";
            stateLabel.Text = Match.CurrentProcedureType.ToString();
        }

        protected override void Update()
        {
            team1Label.X = -scoreText.DrawWidth;
            team2Label.X = scoreText.DrawWidth;
            if (Match.MapProgress.HasValue)
            {
                backgroundFill.Width = (float)Match.MapProgress.Value;
                stateLabel.Text = $"{Match.CurrentProcedureType} ({Match.MapProgressText})";
            }
            else
                backgroundFill.Width = 0;
            base.Update();
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverOverlay.FadeTo(0.3f, 200, Easing.OutCubic);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverOverlay.FadeOut(200, Easing.OutCubic);
            base.OnHoverLost(e);
        }

        public void Select() => background.FadeColour(FrameworkColour.Green, 500, Easing.OutQuart);

        public void Deselect() => background.FadeColour(FrameworkColour.Blue, 250, Easing.OutCubic);
    }
}
