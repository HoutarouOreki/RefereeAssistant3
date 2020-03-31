using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Matches;
using RefereeAssistant3.Main.Storage;
using RefereeAssistant3.Main.Tournaments;
using RefereeAssistant3.Main.Utilities;
using RefereeAssistant3.Visual.UI;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Visual.Overlays.TournamentConfigurationOverlays
{
    public class TournamentConfigurationOverlay : RA3OverlayContainer
    {
        private readonly Bindable<Tournament> selectedTournament = new Bindable<Tournament>();
        private readonly Bindable<TournamentStageConfiguration> selectedStage = new Bindable<TournamentStageConfiguration>();
        private readonly BasicTextBox tournamentNameTextBox;
        private readonly RA3Button stageSelectionButton;
        private readonly SelectionOverlay<TournamentStageConfiguration> stageSelection;
        private readonly BasicTextBox stageNameTextBox;
        private readonly Core core;
        private readonly RA3Button newStageButton;
        private readonly BasicTextBox roomNameTextBox;
        private readonly FillFlowContainer stageFlow;
        private readonly BasicSliderBar<int> scoreToWinSlider;
        private readonly SpriteText scoreToWinText;
        private readonly RA3Button saveStageButton;
        private readonly RA3Button saveTournamentButton;

        public TournamentConfigurationOverlay(Core core)
        {
            this.core = core;
            var tournamentSelection = new SelectionOverlay<Tournament>(core.Tournaments) { Action = t => selectedTournament.Value = t };
            stageSelection = new SelectionOverlay<TournamentStageConfiguration>(new List<TournamentStageConfiguration>()) { Action = s => selectedStage.Value = s };
            Add(tournamentSelection);
            Add(stageSelection);
            Add(new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(Style.SPACING),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(Style.SPACING),
                            Children = new Drawable[]
                            {
                                new RA3Button
                                {
                                    Action = tournamentSelection.Show,
                                    Text = "Select tournament",
                                },
                                new RA3Button
                                {
                                    Action = OnNewTournamentButtonClicked,
                                    Text = "Create new tournament"
                                }
                            }
                        },
                        tournamentNameTextBox = new BasicTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                            PlaceholderText = "Tournament name",
                        },
                        saveTournamentButton = new RA3Button
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Save configuration",
                            Action = SaveConfiguration,
                            BackgroundColour = FrameworkColour.BlueGreen
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(Style.SPACING),
                            Children = new Drawable[]
                            {
                                stageSelectionButton = new RA3Button
                                {
                                    Text = "Select stage"
                                },
                                newStageButton = new RA3Button
                                {
                                    Action = OnNewStageButtonClicked,
                                    Text = "Create new stage"
                                }
                            }
                        },
                        stageNameTextBox = new BasicTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                            PlaceholderText = "Stage name",
                        },
                        stageFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(Style.SPACING),
                            Children = new Drawable[]
                            {
                                roomNameTextBox = new BasicTextBox
                                {
                                    Size = Style.COMPONENTS_SIZE,
                                    PlaceholderText = "Room name"
                                },
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        scoreToWinSlider = new BasicSliderBar<int>
                                        {
                                            Size = Style.COMPONENTS_SIZE,
                                            SelectionColour = FrameworkColour.YellowDark
                                        },
                                        scoreToWinText = new SpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre
                                        }
                                    }
                                }
                            }
                        },
                        saveStageButton = new RA3Button
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Save stage",
                            Action = SaveStage,
                            BackgroundColour = FrameworkColour.BlueGreen
                        },
                    }
                }
            });
            selectedTournament.BindValueChanged(OnTournamentSelected, true);
            selectedStage.BindValueChanged(OnStageChanged, true);
            scoreToWinSlider.Current = new BindableInt
            {
                MaxValue = 50,
                MinValue = 1,
                Precision = 1
            };
            scoreToWinSlider.Current.BindValueChanged(v => scoreToWinText.Text = $"Score to win: {v.NewValue}", true);
        }

        private void SaveConfiguration()
        {
            if (tournamentNameTextBox.Text.Length < 2)
            {
                core.PushAlert("Tournament name too short");
                return;
            }
            if (core.Tournaments.Any(t => Tournament.GetPathFromName(t.Configuration.TournamentName) == Tournament.GetPathFromName(tournamentNameTextBox.Text) && t != selectedTournament.Value))
            {
                core.PushAlert("Tournament name taken");
                return;
            }
            var config = selectedTournament.Value.Configuration;
            config.TournamentName = tournamentNameTextBox.Text;
            selectedTournament.Value.Save();
            GenerateLayout();
        }

        private void SaveStage()
        {
            if (stageNameTextBox.Text.Length < 2)
            {
                core.PushAlert("Stage name too short");
                return;
            }
            var s = selectedStage.Value;
            s.TournamentStageName = stageNameTextBox.Text;
            s.RoomSettings.RoomName = roomNameTextBox.Text;
            s.ScoreRequiredToWin = scoreToWinSlider.Current.Value;
            selectedTournament.Value.Save();
            GenerateLayout();
        }

        private void OnNewStageButtonClicked()
        {
            var stageName = NameUtilities.GetUniqueNewName("New stage", selectedTournament.Value.Stages.Select(existingStage => existingStage.TournamentStageName));
            var stage = new TournamentStageConfiguration
            {
                TournamentStageName = stageName,
                MatchProceedings = new List<string>(),
                ScoreRequiredToWin = 20,
                Mappool = new Mappool(),
                DoFailedScoresCount = false,
                RoomSettings = new MpRoomSettings
                {
                    RoomName = "Room name: (TEAM1) vs (TEAM2)",
                    ScoreMode = ScoreMode.ScoreV2,
                    TeamMode = TeamMode.TeamVs
                }
            };
            selectedTournament.Value.Stages.Add(stage);
            selectedStage.Value = stage;
        }

        private void OnNewTournamentButtonClicked()
        {
            var config = new TournamentConfiguration()
            {
                TournamentName = NameUtilities.GetUniqueNewName("New tournament", core.Tournaments.Select(existingTournament => existingTournament.Configuration.TournamentName))
            };
            var newTournament = new Tournament(config, new List<TournamentStageConfiguration>(), new List<TeamStorage>());
            core.Tournaments.Add(newTournament);
            selectedTournament.Value = newTournament;
        }

        private void OnTournamentSelected(ValueChangedEvent<Tournament> obj)
        {
            selectedStage.Value = null;
            GenerateLayout();
        }

        private void OnStageChanged(ValueChangedEvent<TournamentStageConfiguration> obj) => GenerateLayout();

        private void GenerateLayout()
        {
            var t = selectedTournament.Value;
            tournamentNameTextBox.Text = t?.Configuration.TournamentName;
            stageSelectionButton.Action = newStageButton.Action = saveTournamentButton.Action = saveStageButton.Action = null;
            if (t != null)
            {
                newStageButton.Action = OnNewStageButtonClicked;
                stageSelection.Items = t.Stages;
                stageSelectionButton.Action = stageSelection.Show;
                saveTournamentButton.Action = SaveConfiguration;
            }

            var s = selectedStage.Value;
            stageNameTextBox.Text = s?.TournamentStageName;
            roomNameTextBox.Text = s?.RoomSettings.RoomName;
            if (s != null)
            {
                scoreToWinSlider.Current.Value = s.ScoreRequiredToWin;
                saveStageButton.Action = SaveStage;
            }
        }

        protected override void Update()
        {
            base.Update();
            stageFlow.MaximumSize = ChildSize;
        }
    }
}
