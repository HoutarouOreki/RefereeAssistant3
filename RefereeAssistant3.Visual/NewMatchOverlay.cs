using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using RefereeAssistant3.Main;
using System;

namespace RefereeAssistant3.Visual
{
    public class NewMatchOverlay : RA3OverlayContainer
    {
        private readonly Core core;

        private readonly RA3Button tournamentSelectionButton;
        private readonly RA3Button stageSelectionButton;
        private readonly RA3Button team1SelectionButton;
        private readonly RA3Button team2SelectionButton;
        private readonly SelectionOverlay<Tournament> tournamentSelectionOverlay;
        private readonly SpriteText vsLabel;
        private readonly RA3Button addNewMatchButton;
        private SelectionOverlay<TournamentStage> stageSelectionOverlay;
        private SelectionOverlay<Team> teamSelectionOverlay;

        private Tournament tournament;
        private TournamentStage stage;
        private Team team1;
        private Team team2;

        public NewMatchOverlay(Core core)
        {
            this.core = core;
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.GreenDarker,
                    Alpha = 0.9f
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(Style.SPACING),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(Style.SPACING),
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Children = new Drawable[]
                                {
                                    tournamentSelectionButton = new RA3Button
                                    {
                                        Text = "Select tournament",
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        BackgroundColour = FrameworkColour.Green,
                                        Action = () =>
                                        {
                                            tournamentSelectionOverlay.Action = SetTournament;
                                            tournamentSelectionOverlay.Show();
                                        }
                                    },
                                    stageSelectionButton = new RA3Button
                                    {
                                        Text = "Select tournament's stage",
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        BackgroundColour = FrameworkColour.Green
                                    },
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(Style.SPACING),
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Children = new Drawable[]
                                {
                                    team1SelectionButton = new RA3Button
                                    {
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        BackgroundColour = FrameworkColour.BlueGreen,
                                        AlwaysPresent = true
                                    },
                                    vsLabel = new SpriteText
                                    {
                                        Text = "VS",
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AlwaysPresent = true
                                    },
                                    team2SelectionButton = new RA3Button
                                    {
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        BackgroundColour = FrameworkColour.BlueGreen,
                                        AlwaysPresent = true
                                    }
                                }
                            }
                        },
                        addNewMatchButton = new RA3Button
                        {
                            Text = "Add match",
                            Width = Style.COMPONENTS_WIDTH,
                            Height = Style.COMPONENTS_HEIGHT,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            BackgroundColour = FrameworkColour.Green,
                            AlwaysPresent = true
                        }
                    }
                },
                tournamentSelectionOverlay = new SelectionOverlay<Tournament>(core.Tournaments)
            };
            tournamentSelectionOverlay.Hide();
        }

        protected override void PopIn()
        {
            UpdateDisplay();
            base.PopIn();
        }

        private bool AreOptionsValid()
        {
            if (team1 == null || team2 == null || team1 == team2 || !tournament.Teams.Contains(team1) || !tournament.Teams.Contains(team2) || !tournament.Stages.Contains(stage))
                return false;
            return true;
        }

        private void AddNewMatch()
        {
            if (!AreOptionsValid())
                return;
            var match = new Match(team1, team2, tournament, stage);
            core.AddNewMatch(match);
            Hide();
            tournament = null;
            stage = null;
            team1 = null;
            team2 = null;
            UpdateDisplay();
        }

        private void SetTournament(Tournament tournament)
        {
            this.tournament = tournament;
            stage = null;
            team1 = team2 = null;
            if (stageSelectionOverlay != null)
                Remove(stageSelectionOverlay);
            Add(stageSelectionOverlay = new SelectionOverlay<TournamentStage>(tournament.Stages));
            stageSelectionOverlay.Hide();
            stageSelectionButton.Action = () =>
            {
                stageSelectionOverlay.Action = SetStage;
                stageSelectionOverlay.Show();
            };
            UpdateDisplay();
        }

        private void SetStage(TournamentStage stage)
        {
            this.stage = stage;
            team1 = team2 = null;
            if (teamSelectionOverlay != null)
                Remove(teamSelectionOverlay);
            stageSelectionButton.Text = "Loading...";

            // async because JIT takes too long on the first run
            LoadComponentAsync(teamSelectionOverlay = new SelectionOverlay<Team>(tournament.Teams), d =>
            {
                Add(d);
                teamSelectionOverlay.Hide();
                team1SelectionButton.Action = () =>
                {
                    teamSelectionOverlay.Action = SetTeam1;
                    teamSelectionOverlay.Show();
                };
                team2SelectionButton.Action = () =>
                {
                    teamSelectionOverlay.Action = SetTeam2;
                    teamSelectionOverlay.Show();
                };
                UpdateDisplay();
            });
        }

        private void SetTeam1(Team team)
        {
            team1 = team;
            UpdateDisplay();
        }

        private void SetTeam2(Team team)
        {
            team2 = team;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            tournamentSelectionButton.Text = tournament?.TournamentName ?? "Select the tournament";
            stageSelectionButton.Text = stage?.TournamentStageName ?? "Select the stage";
            team1SelectionButton.Text = team1?.TeamName ?? "Select team 1";
            team2SelectionButton.Text = team2?.TeamName ?? "Select team 2";

            if (stage == null)
                team1SelectionButton.Action = team2SelectionButton.Action = null;

            addNewMatchButton.Action = AreOptionsValid() ? AddNewMatch : (Action)null;

            vsLabel.FadeTo(stage != null ? 1 : 0.5f);
        }
    }
}
