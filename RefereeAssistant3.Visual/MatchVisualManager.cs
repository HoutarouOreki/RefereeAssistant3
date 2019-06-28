using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using RefereeAssistant3.Main;
using System;
using System.Linq;

namespace RefereeAssistant3.Visual
{
    public class MatchVisualManager : Container
    {
        private const int proceed_button_width = 240;
        private static readonly float team_name_score_height = 84;
        private static readonly float team_name_score_padding = 18;
        private static readonly float team_name_score_font_size = team_name_score_height - (2 * team_name_score_padding);
        private static readonly float match_state_height = 42;

        public Match Match
        {
            get => match;
            set
            {
                if (match != null)
                    match.Updated -= OnMatchUpdate;
                match = value;
                match.Updated += OnMatchUpdate;
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
        private readonly RA3Button mapPickerButton;
        private readonly MapPickerOverlay mapPicker;
        private readonly TeamButton team1Button;
        private readonly TeamButton team2Button;
        private readonly RA3Button proceedButton;

        public MatchVisualManager(MapPickerOverlay mapPicker)
        {
            this.mapPicker = mapPicker;
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
                            Alpha = 0.4f
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
                            Alpha = 0.4f
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
                            Colour = FrameworkColour.Yellow.Darken(1),
                        },
                        matchStateLabel = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                },
                matchControls = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = match_state_height * 0.7f,
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
                                    EdgeSmoothness = new Vector2(1),
                                    Colour = Color4.Black
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Rotation = 3,
                                    EdgeSmoothness = new Vector2(1),
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
                        new CircularContainer
                        {
                            Size = new Vector2(proceed_button_width + 2),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            MaskingSmoothness = 3,
                            Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black }
                        },
                        new CircularContainer
                        {
                            Size = new Vector2(proceed_button_width),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Child = proceedButton = new RA3Button
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                Size = new Vector2(1, 0.6f),
                                BackgroundColour = FrameworkColour.Blue,
                                Text = "Proceed"
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = proceed_button_width / 2,
                            Depth = 3,
                            Children = new Drawable[]
                            {
                                team1Button = new TeamButton(true, proceed_button_width / 2)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopRight,
                                    BackgroundColour = Style.Red.Darken(0.8f),
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0.5f, 1),
                                    X = -0.5f
                                },
                                team2Button = new TeamButton(false, proceed_button_width / 2)
                                {
                                    BackgroundColour = Style.Blue.Darken(0.8f),
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(0.5f, 1),
                                    Anchor = Anchor.TopCentre,
                                    X = 0.5f
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = proceed_button_width / 2,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    BackgroundColour = FrameworkColour.Green,
                                    Text = "History"
                                },
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    BackgroundColour = FrameworkColour.Green,
                                    Text = "Undo last action"
                                },
                                mapPickerButton = new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    BackgroundColour = FrameworkColour.Green,
                                    Text = "Pick map"
                                },
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    BackgroundColour = FrameworkColour.Green,
                                    Text = "Preset messages"
                                }
                            }
                        }
                    }
                }
            };
        }

        private void GenerateLayout()
        {
            team1ScoreBox.Label.Text = match.Scores[match.Team1].ToString();
            team1NameLabel.Text = match.Team1.TeamName;
            team2ScoreBox.Label.Text = match.Scores[match.Team2].ToString();
            team2NameLabel.Text = match.Team2.TeamName;

            tournamentLabel.Text = match.Tournament.TournamentName;
            stageLabel.Text = match.TournamentStage.TournamentStageName;

            matchStateLabel.Text = "";
            matchStateLabel.AddText(match.ReadableCurrentState);

            if (new[] { MatchProcedure.Banning1, MatchProcedure.Banning2, MatchProcedure.BanningRollWinner, MatchProcedure.BanningRollLoser, MatchProcedure.Picking1, MatchProcedure.Picking2, MatchProcedure.PickingRollWinner, MatchProcedure.PickingRollLoser, MatchProcedure.WarmUp1, MatchProcedure.WarmUp2, MatchProcedure.WarmUpRollWinner, MatchProcedure.WarmUpRollLoser }.Contains(Match.CurrentProcedure))
            {
                mapPickerButton.Action = mapPicker.Show;
            }
            else
                mapPickerButton.Action = null;

            team1Button.Action = team2Button.Action = null;
            team1Button.Text.Text = team2Button.Text.Text = null;

            proceedButton.Text = null;
            proceedButton.Action = null;

            switch (Match.CurrentProcedure)
            {
                case MatchProcedure.SettingUp:
                    OnSettingUpProcedure();
                    break;
                case MatchProcedure.WarmUp1:
                    break;
                case MatchProcedure.WarmUp2:
                    break;
                case MatchProcedure.WarmUpRollWinner:
                    break;
                case MatchProcedure.WarmUpRollLoser:
                    break;
                case MatchProcedure.Rolling:
                    OnRollingProcedure();
                    break;
                case MatchProcedure.Banning1:
                case MatchProcedure.Banning2:
                case MatchProcedure.BanningRollWinner:
                case MatchProcedure.BanningRollLoser:
                case MatchProcedure.Picking1:
                case MatchProcedure.Picking2:
                case MatchProcedure.PickingRollWinner:
                case MatchProcedure.PickingRollLoser:
                    OnBanningOrPickingProcedure();
                    break;
                case MatchProcedure.GettingReady:
                    OnGettingReadyProcedure();
                    break;
                case MatchProcedure.TieBreaker:
                    break;
                case MatchProcedure.Playing:
                    break;
                case MatchProcedure.FreePoint1:
                    break;
                case MatchProcedure.FreePoint2:
                    break;
                case MatchProcedure.FreePointRollWinner:
                    break;
                case MatchProcedure.FreePointRollLoser:
                    break;
                default:
                    break;
            }
        }

        private void EnableProceedButton(string text = null)
        {
            proceedButton.Action = () => Match.Proceed();
            if (text != null)
                proceedButton.Text = text;
        }

        private void OnSettingUpProcedure() => EnableProceedButton("Finish setting up");

        private void OnRollingProcedure()
        {
            team1Button.Action = () =>
            {
                Match.RollWinner = Match.Team1;
                GenerateLayout();
            };
            team1Button.Text.Text = $"{Match.Team1} won roll";
            team2Button.Action = () =>
            {
                Match.RollWinner = Match.Team2;
                GenerateLayout();
            };
            team2Button.Text.Text = $"{Match.Team2} won roll";
            if (Match.RollWinner != null)
            {
                EnableProceedButton($"Set {Match.RollWinner}\nas roll winner");
                if (Match.Team1 == Match.RollWinner)
                    team1Button.IsSelected = true;
                if (Match.Team2 == Match.RollWinner)
                    team2Button.IsSelected = true;
            }
        }

        private void OnBanningOrPickingProcedure()
        {
            if (Match.SelectedMap != null)
            {
                EnableProceedButton("Proceed");
                matchStateLabel.Text = $"{Match.ReadableCurrentState}: {Match.SelectedMap}";
            }
        }

        private void OnGettingReadyProcedure() => EnableProceedButton("Start match");

        protected override void Update()
        {
            matchControls.Height = DrawHeight - (2 * team_name_score_height) - match_state_height;
            base.Update();
        }

        private void OnMatchUpdate() => GenerateLayout();

        private class ScoreNumberBox : Container
        {
            public readonly SpriteText Label;

            public ScoreNumberBox()
            {
                Size = new Vector2(team_name_score_height);
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
