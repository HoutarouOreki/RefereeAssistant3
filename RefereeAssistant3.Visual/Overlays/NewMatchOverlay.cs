using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Matches;
using RefereeAssistant3.Main.Storage;
using RefereeAssistant3.Main.Tournaments;
using RefereeAssistant3.Visual.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Visual.Overlays
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
        private readonly FillFlowContainer team1MembersDisplay;
        private readonly FillFlowContainer team2MembersDisplay;
        private readonly Container teamMembersDisplay;
        private SelectionOverlay<TournamentStageConfiguration> stageSelectionOverlay;
        private SelectionOverlay<TeamStorage> teamSelectionOverlay;

        private Tournament tournament;
        private TournamentStageConfiguration stage;
        private TeamStorage team1;
        private TeamStorage team2;

        public NewMatchOverlay(Core core)
        {
            this.core = core;
            Children = new Drawable[]
            {
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
                        teamMembersDisplay = new Container
                        {
                            Width = 70,
                            AutoSizeAxes = Axes.Y,
                            AutoSizeDuration = 200,
                            AutoSizeEasing = Easing.InOutCubic,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                team1MembersDisplay = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(6),
                                    Direction = FillDirection.Vertical
                                },
                                team2MembersDisplay = new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(6),
                                    Direction = FillDirection.Vertical
                                },
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
            tournament = null;
            stage = null;
            team1 = team2 = null;
            UpdateDisplay();
            base.PopIn();
        }

        private bool AreOptionsValid()
        {
            if (team1 == null || team2 == null || team1.Equals(team2) || !tournament.Teams?.Any(t => t.Equals(team1)) == true || !tournament?.Teams.Any(t => t.Equals(team2)) == true || !tournament.Stages.Contains(stage))
                return false;
            return true;
        }

        private void AddNewMatch()
        {
            if (!AreOptionsValid())
                return;
            var match = new OsuTeamVsMatch(team1, team2, tournament, stage);
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
            Add(stageSelectionOverlay = new SelectionOverlay<TournamentStageConfiguration>(tournament.Stages));
            stageSelectionOverlay.Hide();
            stageSelectionButton.Action = () =>
            {
                stageSelectionOverlay.Action = SetStage;
                stageSelectionOverlay.Show();
            };
            UpdateDisplay();
        }

        private void SetStage(TournamentStageConfiguration stage)
        {
            this.stage = stage;
            team1 = team2 = null;
            if (teamSelectionOverlay != null)
                Remove(teamSelectionOverlay);
            stageSelectionButton.Text = "Loading...";

            // async because JIT takes too long on the first run
            LoadComponentAsync(teamSelectionOverlay = new SelectionOverlay<TeamStorage>(tournament.Teams), d =>
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

        private void SetTeam1(TeamStorage team)
        {
            team1 = team;
            UpdateDisplay();
        }

        private void SetTeam2(TeamStorage team)
        {
            team2 = team;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            tournamentSelectionButton.Text = tournament?.Configuration.TournamentName ?? "Select the tournament";
            stageSelectionButton.Text = stage?.TournamentStageName ?? "Select the stage";
            team1SelectionButton.Text = team1?.TeamName ?? "Select team 1";
            team2SelectionButton.Text = team2?.TeamName ?? "Select team 2";

            if (tournament == null)
                stageSelectionButton.Action = null;

            if (tournament == null || stage == null)
                team1SelectionButton.Action = team2SelectionButton.Action = null;

            addNewMatchButton.Action = AreOptionsValid() ? AddNewMatch : (Action)null;
            addNewMatchButton.Text = stage != null ? $"Create new {stage.RoomSettings.TeamMode} match" : "Create new match";

            vsLabel.FadeTo(stage != null ? 1 : 0.5f);

            team1MembersDisplay.Clear();
            team2MembersDisplay.Clear();

            if (team1 != null)
            {
                team1MembersDisplay.Child = new SpriteText
                { Text = "Loading team members..", Anchor = Anchor.TopLeft, Origin = Anchor.TopLeft };
                var componentsToLoad = new List<AvatarUsernameLine>();
                foreach (var member in team1.Members)
                    componentsToLoad.Add(new AvatarUsernameLine(new Player(member.PlayerId), false));
                LoadComponentsAsync(componentsToLoad, ds => team1MembersDisplay.ChildrenEnumerable = ds);
            }

            if (team2 != null)
            {
                team2MembersDisplay.Child = new SpriteText
                { Text = "Loading team members..", Anchor = Anchor.TopRight, Origin = Anchor.TopRight };
                var componentsToLoad = new List<AvatarUsernameLine>();
                foreach (var member in team2.Members)
                    componentsToLoad.Add(new AvatarUsernameLine(new Player(member.PlayerId), true));
                LoadComponentsAsync(componentsToLoad, ds => team2MembersDisplay.ChildrenEnumerable = ds);
            }

            teamMembersDisplay.Width = vsLabel.DrawWidth + (Style.SPACING * 2) + 5;
        }
    }
}
