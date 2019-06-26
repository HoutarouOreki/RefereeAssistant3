using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class NewMatchOverlay : RA3OverlayContainer
    {
        private readonly Core core;

        private readonly RA3Button team1SelectionButton;
        private readonly RA3Button team2SelectionButton;
        private readonly SelectionOverlay<Team> selectionOverlay;

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
                    Colour = FrameworkColour.BlueGreenDark,
                    Alpha = 0.8f
                },
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(Style.SPACING),
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
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
                                        team1SelectionButton = new RA3Button
                                        {
                                            Width = Style.COMPONENTS_WIDTH,
                                            Height = Style.COMPONENTS_HEIGHT,
                                            BackgroundColour = FrameworkColour.BlueGreen,
                                            Action = () =>
                                            {
                                                selectionOverlay.Action = SetTeam1;
                                                selectionOverlay.Show();
                                            }
                                        },
                                        new SpriteText
                                        {
                                            Text = "VS",
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft
                                        },
                                        team2SelectionButton = new RA3Button
                                        {
                                            Width = Style.COMPONENTS_WIDTH,
                                            Height = Style.COMPONENTS_HEIGHT,
                                            BackgroundColour = FrameworkColour.BlueGreen,
                                            Action = () =>
                                            {
                                                selectionOverlay.Action = SetTeam2;
                                                selectionOverlay.Show();
                                            }
                                        },
                                    }
                                },
                            },
                            new RA3Button
                            {
                                Text = "Add match",
                                Action = AddNewMatch,
                                Width = Style.COMPONENTS_WIDTH,
                                Height = Style.COMPONENTS_HEIGHT,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                BackgroundColour = FrameworkColour.Green,
                            }
                        }
                    }
                },
                selectionOverlay = new SelectionOverlay<Team>(core.Teams)
            };
            selectionOverlay.Hide();
        }

        protected override void PopIn()
        {
            UpdateDisplay();
            base.PopIn();
        }

        private void AddNewMatch()
        {
            if (team1 == null || team2 == null || team1 == team2)
                return;
            var match = new Match(team1, team2, new Mappool(), TournamentStage.Qualifiers);
            core.AddNewMatch(match);
            Hide();
            team1 = null;
            team2 = null;
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
            team1SelectionButton.Text = team1?.TeamName ?? "Team 1 unselected";
            team2SelectionButton.Text = team2?.TeamName ?? "Team 2 unselected";
        }
    }
}
