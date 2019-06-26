using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class MatchVisualManager : Container
    {
        private static readonly float team_name_score_height = 84;
        private static readonly float team_name_score_padding = 18;
        private static readonly float team_name_score_font_size = team_name_score_height - (2 * team_name_score_padding);
        private static readonly float match_state_height = 42;

        public Match Match
        {
            get => match;
            set
            {
                match = value;
                GenerateLayout();
            }
        }
        private Match match;
        private readonly ScoreNumberBox team1ScoreBox;
        private readonly ScoreNumberBox team2ScoreBox;
        private readonly SpriteText team1NameLabel;
        private readonly SpriteText team2NameLabel;
        private readonly TextFlowContainer matchStateLabel;
        private readonly Container matchControls;
        private readonly SpriteText tournamentLabel;
        private readonly SpriteText stageLabel;

        public MatchVisualManager()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = team_name_score_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Style.Red,
                            Alpha = 0.3f
                        },
                        team1ScoreBox = new ScoreNumberBox(),
                        team1NameLabel = new SpriteText
                        {
                            X = team_name_score_height + team_name_score_padding,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = new FontUsage(null, team_name_score_font_size)
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Y = team_name_score_height,
                    Height = team_name_score_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Style.Blue,
                            Alpha = 0.3f
                        },
                        team2ScoreBox = new ScoreNumberBox { Anchor = Anchor.TopRight, Origin = Anchor.TopRight },
                        team2NameLabel = new SpriteText
                        {
                            X = -(team_name_score_height + team_name_score_padding),
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Font = new FontUsage(null, team_name_score_font_size)
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = match_state_height,
                    Y = 2 * team_name_score_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FrameworkColour.Yellow,
                            Alpha = 0.42f
                        },
                        matchStateLabel = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = match_state_height * 0.7f,
                    Y = (2 * team_name_score_height) + match_state_height,
                    Depth = 2,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Rotation = -3,
                            EdgeSmoothness = new osuTK.Vector2(1),
                            Colour = Color4.Black
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Rotation = 3,
                            EdgeSmoothness = new osuTK.Vector2(1),
                            Colour = Color4.Black
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Horizontal = 8, Vertical = 4 },
                            Children = new Drawable[]
                            {
                                tournamentLabel = new SpriteText
                                {
                                    Font = new FontUsage(null, 15),
                                    Colour = Color4.Gray
                                },
                                stageLabel = new SpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Font = new FontUsage(null, 15),
                                    Colour = Color4.Gray
                                }
                            }
                        }
                    }
                },
                matchControls = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre
                }
            };
        }

        private void GenerateLayout()
        {
            team1ScoreBox.Label.Text = match.Score[match.Team1].ToString();
            team1NameLabel.Text = match.Team1.TeamName;
            team2ScoreBox.Label.Text = match.Score[match.Team2].ToString();
            team2NameLabel.Text = match.Team2.TeamName;

            tournamentLabel.Text = match.Tournament.TournamentName;
            stageLabel.Text = match.TournamentStage.TournamentStageName;

            matchStateLabel.Text = "";
            matchStateLabel.AddText(match.ReadableCurrentState);
        }

        protected override void Update()
        {
            matchControls.Height = DrawHeight - (2 * team_name_score_height) - match_state_height;
            base.Update();
        }

        private class ScoreNumberBox : Container
        {
            public readonly SpriteText Label;

            public ScoreNumberBox()
            {
                Size = new osuTK.Vector2(team_name_score_height);
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.2f
                    },
                    Label = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = new FontUsage(null, team_name_score_font_size)
                    }
                };
            }
        }
    }
}
