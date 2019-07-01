using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;
using RefereeAssistant3.Main;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3.Visual
{
    public class MatchVisualManager : Container
    {
        private const int proceed_button_width = 240;
        private const float team_name_score_height = 84;
        private const float team_name_score_padding = 18;
        private const float team_name_score_font_size = team_name_score_height - (2 * team_name_score_padding);
        private const float match_state_height = 42;
        private static readonly Color4 proceed_button_default_colour = FrameworkColour.Green.Darken(0.5f);

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
        private TextureStore textures;
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
        private readonly MapFinderOverlay mapFinder;
        private readonly MatchPostOverlay postOverlay;
        private readonly TeamButton team1Button;
        private readonly TeamButton team2Button;
        private readonly RA3Button proceedButton;
        private readonly Container matchStateContainer;
        private readonly Container selectedMapDisplayContainer;
        private readonly TextFlowContainer currentMapLabel;
        private readonly Sprite currentMapCover;
        private readonly Core core;
        private readonly RA3Button matchSubmissionButton;
        private readonly RA3Button undoButton;
        private readonly SpriteText team1BansText;
        private readonly SpriteText team1PicksText;
        private readonly SpriteText team2BansText;
        private readonly SpriteText team2PicksText;
        private readonly ChatContainer chatContainer;

        public MatchVisualManager(Core core, MapPickerOverlay mapPicker, MapFinderOverlay mapFinder, MatchPostOverlay postOverlay)
        {
            this.core = core;
            this.mapPicker = mapPicker;
            this.mapFinder = mapFinder;
            this.postOverlay = postOverlay;
            RelativeSizeAxes = Axes.Both;
            Masking = true;
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
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Children = new Drawable[]
                            {
                                team1PicksText = new SpriteText
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Font = new FontUsage(null, 16)
                                },
                                team1BansText = new SpriteText
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    Font = new FontUsage(null, 16)
                                }
                            }
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
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Children = new Drawable[]
                            {
                                team2PicksText = new SpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Font = new FontUsage(null, 16)
                                },
                                team2BansText = new SpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Font = new FontUsage(null, 16)
                                }
                            }
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
                        matchStateContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = FrameworkColour.YellowDark
                                },
                                matchStateLabel = new TextFlowContainer(t => t.Font = new FontUsage("OpenSans-Bold", 18))
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
                        },
                        selectedMapDisplayContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.5f,
                            Anchor = Anchor.TopRight,
                            RelativePositionAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = FrameworkColour.Blue.Darken(1)
                                },
                                currentMapCover = new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fill,
                                    Alpha = 0.5f
                                },
                                currentMapLabel = new TextFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                }
                            }
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
                            Size = new Vector2(proceed_button_width + 1),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            MaskingSmoothness = 1,
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
                                BackgroundColour = proceed_button_default_colour,
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
                            Depth = 2,
                            Children = new Drawable[]
                            {
                                matchSubmissionButton = new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    Text = "Match submission"
                                },
                                undoButton = new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    Text = "Undo last action"
                                },
                                mapPickerButton = new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    Text = "Pick map"
                                },
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Size = new Vector2(1/4f, Style.COMPONENTS_HEIGHT),
                                    Text = "Preset messages"
                                }
                            }
                        }
                    }
                },
                chatContainer = new ChatContainer(core)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                }
            };
        }

        private void GenerateLayout()
        {
            team1ScoreBox.Label.Text = match.Scores[match.Team1].ToString();
            team1NameLabel.Text = match.Team1.TeamName;
            team1BansText.Text = match.Team1.BannedMaps.Count > 0 ? $"Bans: {string.Join(' ', match.Team1.BannedMaps.Select(m => m.MapCode))}" : null;
            team1PicksText.Text = match.Team1.PickedMaps.Count > 0 ? $"Picks: {string.Join(' ', match.Team1.PickedMaps.Select(m => m.MapCode))}" : null;
            team2ScoreBox.Label.Text = match.Scores[match.Team2].ToString();
            team2NameLabel.Text = match.Team2.TeamName;
            team2BansText.Text = match.Team2.BannedMaps.Count > 0 ? $"Bans: {string.Join(' ', match.Team2.BannedMaps.Select(m => m.MapCode))}" : null;
            team2PicksText.Text = match.Team2.PickedMaps.Count > 0 ? $"Picks: {string.Join(' ', match.Team2.PickedMaps.Select(m => m.MapCode))}" : null;

            tournamentLabel.Text = match.Tournament.TournamentName;
            stageLabel.Text = match.TournamentStage.TournamentStageName;

            matchStateLabel.Text = "";
            matchStateLabel.AddText(match.ReadableCurrentState);

            ColourProceedButton(null);

            const Easing easing = Easing.OutCubic;
            const float duration = 200;
            var mapLabelWidth = 0.5f;
            selectedMapDisplayContainer.MoveToX(Match.SelectedMap == null ? 0 : -mapLabelWidth, duration, easing)
                .OnComplete(d =>
                {
                    if (Match.SelectedMap == null)
                        currentMapCover.Texture = null;
                });
            matchStateContainer.ResizeWidthTo(Match.SelectedMap == null ? 1 : 1 - mapLabelWidth, duration, easing);
            currentMapLabel.Text = Match.SelectedMap != null ?
                $"({Match.SelectedMap.MapCode}) {Match.SelectedMap}" : "";

            matchSubmissionButton.Action = null;
            matchSubmissionButton.Text = "";
            if (string.IsNullOrEmpty(MainConfig.ServerURL))
                matchSubmissionButton.Text = "Server URL not set";
            else if (!Uri.IsWellFormedUriString(MainConfig.ServerURL, UriKind.Absolute))
                matchSubmissionButton.Text = "Server URL invalid";
            else if (Match?.Id == -1)
            {
                matchSubmissionButton.Action = postOverlay.Show;
                matchSubmissionButton.Text = "Upload match";
            }
            else if (Match?.Id > -1 && Match.ModifiedSinceUpdate)
            {
                matchSubmissionButton.Action = () =>
                {
                    matchSubmissionButton.Text = "Submitting update...";
                    Task.Run(core.UpdateMatchAsync);
                };
                matchSubmissionButton.Text = "Submit match update";
            }
            else if (Match?.Id > -1 && !Match.ModifiedSinceUpdate)
            {
                matchSubmissionButton.Action = null;
                matchSubmissionButton.Text = "Match uploaded";
            }

            Task.Run(() =>
            {
                if (Match?.SelectedMap != null)
                    currentMapCover.Texture = Match?.SelectedMap?.DownloadCover(textures);
            });
            if (Match?.SelectedMap != null)
                currentMapCover.FadeTo(0.5f, 200);

            undoButton.Action = Match.History.Count > 0 ? Match.ReverseLastOperation : (Action)null;

            team1Button.Action = team2Button.Action = null;
            team1Button.Text.Text = team2Button.Text.Text = null;

            proceedButton.Text = null;
            proceedButton.Action = null;

            mapPickerButton.Action = null;

            if (Match.IsFinished)
            {
                matchStateLabel.Text = $"{Match.Winner} won the match";
                return;
            }

            switch (Match.CurrentProcedure)
            {
                case MatchProcedure.SettingUp:
                    OnSettingUpProcedure();
                    break;
                case MatchProcedure.WarmUp1:
                case MatchProcedure.WarmUp2:
                case MatchProcedure.WarmUpRollWinner:
                case MatchProcedure.WarmUpRollLoser:
                    OnWarmUpProcedure();
                    break;
                case MatchProcedure.Rolling:
                    OnRollingProcedure();
                    break;
                case MatchProcedure.Banning1:
                case MatchProcedure.Banning2:
                case MatchProcedure.BanningRollWinner:
                case MatchProcedure.BanningRollLoser:
                    OnBanningProcedure();
                    break;
                case MatchProcedure.Picking1:
                case MatchProcedure.Picking2:
                case MatchProcedure.PickingRollWinner:
                case MatchProcedure.PickingRollLoser:
                    OnPickingProcedure();
                    break;
                case MatchProcedure.GettingReady:
                    OnGettingReadyProcedure();
                    break;
                case MatchProcedure.TieBreaker:
                    break;
                case MatchProcedure.Playing:
                    OnPlayingProcedure();
                    break;
                case MatchProcedure.PlayingWarmUp:
                    OnPlayingWarmUpProcedure();
                    break;
                case MatchProcedure.FreePoint1:
                case MatchProcedure.FreePoint2:
                case MatchProcedure.FreePointRollWinner:
                case MatchProcedure.FreePointRollLoser:
                    OnFreePointProcedure();
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
            team1Button.Action = () => Match.RollWinner = Match.Team1;
            team1Button.Text.Text = $"{Match.Team1} won roll";
            team2Button.Action = () => Match.RollWinner = Match.Team2;
            team2Button.Text.Text = $"{Match.Team2} won roll";
            if (Match.RollWinner != null)
            {
                EnableProceedButton($"Set {Match.RollWinner}\nas roll winner");
                ColourProceedButton(Match.RollWinner);
                if (Match.Team1 == Match.RollWinner)
                    team1Button.IsSelected = true;
                if (Match.Team2 == Match.RollWinner)
                    team2Button.IsSelected = true;
            }
        }

        private void OnWarmUpProcedure()
        {
            mapPickerButton.Action = mapFinder.Show;
            if (Match.SelectedMap != null)
                EnableProceedButton($"Confirm warmup pick");
        }

        private void OnBanningProcedure()
        {
            mapPickerButton.Action = mapPicker.Show;
            if (Match.SelectedMap != null)
                EnableProceedButton($"Confirm ban");
        }

        private void OnPickingProcedure()
        {
            mapPickerButton.Action = mapPicker.Show;
            if (Match.SelectedMap != null)
                EnableProceedButton($"Confirm pick");
        }

        private void OnGettingReadyProcedure() => EnableProceedButton("Start match");

        private void OnPlayingProcedure()
        {
            team1Button.Text.Text = $"{Match.Team1} won";
            team2Button.Text.Text = $"{Match.Team2} won";
            team1Button.Action = () =>
            {
                Match.SelectedWinner = Match.Team1;
                ColourProceedButton(Match.Team1);
            };
            team2Button.Action = () =>
            {
                Match.SelectedWinner = Match.Team2;
                ColourProceedButton(Match.Team2);
            };
            if (Match.SelectedWinner != null)
            {
                EnableProceedButton($"Proceed");
                ColourProceedButton(Match.SelectedWinner);
                if (Match.Team1 == Match.SelectedWinner)
                    team1Button.IsSelected = true;
                if (Match.Team2 == Match.SelectedWinner)
                    team2Button.IsSelected = true;
            }
        }

        private void OnPlayingWarmUpProcedure() => EnableProceedButton("Finish warmup");

        private void OnFreePointProcedure() => EnableProceedButton("Confirm point");

        private void ColourProceedButton(Team team)
        {
            const float darken_amount = 1f;
            if (team == Match.Team1)
                proceedButton.BackgroundColour = Style.Red.Darken(darken_amount);
            else if (team == Match.Team2)
                proceedButton.BackgroundColour = Style.Blue.Darken(darken_amount);
            else
                proceedButton.BackgroundColour = proceed_button_default_colour;
        }

        protected override void Update()
        {
            matchControls.Height = DrawHeight - (2 * team_name_score_height) - match_state_height;

            if ((Match?.CurrentProcedure == MatchProcedure.Playing || Match?.CurrentProcedure == MatchProcedure.PlayingWarmUp) && Match.SelectedMap?.Length > 0)
                matchStateLabel.Text = $@"{Match.ReadableCurrentState} ({Match.MapProgressText})";

            chatContainer.Height = DrawHeight - (team_name_score_height * 2) - (proceed_button_width / 2) - match_state_height - Style.COMPONENTS_HEIGHT;
            base.Update();
        }

        private void OnMatchUpdate() => Schedule(GenerateLayout);

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

        [BackgroundDependencyLoader]
        private void Load(TextureStore textures) => this.textures = textures;
    }
}
