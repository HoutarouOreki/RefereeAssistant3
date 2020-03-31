using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Matches;
using RefereeAssistant3.Visual.Overlays;
using RefereeAssistant3.Visual.UI;
using System;

namespace RefereeAssistant3.Visual
{
    public class RefereeAssistant3Visual : Game
    {
        private const float match_list_width = 396;
        private const float match_list_controls_height = 3.5f * Style.COMPONENTS_HEIGHT;
        private readonly Core core;
        private FillFlowContainer<MatchPreviewPanel> matchListDisplayer;
        private TeamVsMatchVisualManager matchVisualManager;

        public new GameHost Host => base.Host;

        public RefereeAssistant3Visual(Core core)
        {
            this.core = core;
            core.Alert += OnAlert;
            VisualConfig.Load();
        }

        private void OnAlert(string obj) => Schedule(() => ShowAlert(obj));

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (Window.WindowState == VisualConfig.WindowState)
                return;
            VisualConfig.WindowState = Window.WindowState;
            VisualConfig.Save();
        }

        public DependencyContainer DependencyContainer;
        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override void LoadComplete()
        {
            DependencyContainer.CacheAs(this);

            Window.Title = "Referee Assistant 3";

            Window.WindowState = VisualConfig.WindowState != WindowState.Minimized ? VisualConfig.WindowState : WindowState.Normal;
            Window.WindowStateChanged += OnWindowStateChanged;

            Resources.AddStore(new DllResourceStore(@"RefereeAssistant3.Resources.dll"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Hangul"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Compatibility"));

            base.LoadComplete();
            // doing this in the initializer throws
            var newMatchOverlay = new NewMatchOverlay(core);
            var settingsOverlay = new SettingsOverlay();
            var mapPickerOverlay = new MapPickerOverlay(core);
            var mapFinderOverlay = new MapFinderOverlay(core);
            var matchPostOverlay = new MatchPostOverlay(core);
            var tournamentsOverlay = new TournamentsOverlay(core);
            Children = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.Both, Colour = FrameworkColour.GreenDarker },
                new Container // main content
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = match_list_width },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = matchVisualManager = new TeamVsMatchVisualManager(core, mapPickerOverlay, mapFinderOverlay, matchPostOverlay)
                        }
                    }
                },
                new Container // match list
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Width = match_list_width,
                    Children = new Drawable[]
                    {
                        new Container // scroll container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Bottom = match_list_controls_height },
                            Child = new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = matchListDisplayer = new FillFlowContainer<MatchPreviewPanel>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(2),
                                    Direction = FillDirection.Vertical
                                }
                            }
                        },
                        new FillFlowContainer // controls container
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Height = match_list_controls_height,
                            Children = new Drawable[]
                            {
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 1,
                                    BackgroundColour = FrameworkColour.Green,
                                    Text = "Add new match",
                                    Action = newMatchOverlay.Show
                                },
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 1,
                                    BackgroundColour = FrameworkColour.BlueGreen,
                                    Text = "Configure Tournaments",
                                    Action = tournamentsOverlay.Show
                                },
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 1,
                                    BackgroundColour = FrameworkColour.BlueGreen,
                                    Text = "Settings",
                                    Action = settingsOverlay.Show,
                                }
                            }
                        },
                        new Box { RelativeSizeAxes = Axes.Y }
                    }
                },
                newMatchOverlay,
                settingsOverlay,
                mapPickerOverlay,
                mapFinderOverlay,
                matchPostOverlay,
                tournamentsOverlay
            };
            foreach (var match in core.Matches)
                OnNewMatchAdded(match);
            core.NewMatchAdded += OnNewMatchAdded;
            if (string.IsNullOrEmpty(MainConfig.APIKey))
                ShowAlert("API Key not provided. Some functionality may not be available.", "Open settings", () => settingsOverlay.Show());
        }

        private void OnNewMatchAdded(OsuMatch match)
        {
            Schedule(() =>
            {
                var matchPreviewPanel = new MatchPreviewPanel(match)
                {
                    Action = () => SelectMatch(match)
                };
                matchListDisplayer.Add(matchPreviewPanel);
                match.Alert += OnMatchAlert;
                SelectMatch(match);
            });
        }

        private void OnMatchAlert(OsuMatch source, string text)
        {
            Schedule(() =>
            {
                if (source == core.SelectedMatch.Value)
                {
                    var alert = new Alert(text);
                    Add(alert);
                    alert.Show();
                }
            });
        }

        public void ShowAlert(string text)
        {
            var alert = new Alert(text) { Depth = float.MinValue };
            Add(alert);
            alert.Show();
        }

        public void ShowAlert(string message, string buttonMessage, Action buttonAction)
        {
            var alert = new Alert(message, buttonMessage, buttonAction);
            Add(alert);
            alert.Show();
        }

        private void SelectMatch(OsuMatch match)
        {
            core.SelectedMatch.Value = match;
            foreach (var matchPanel in matchListDisplayer)
            {
                if (matchPanel.Match == core.SelectedMatch.Value)
                    matchPanel.Select();
                else
                    matchPanel.Deselect();
            }
            if (match  is OsuTeamVsMatch teamVsMatch)
                matchVisualManager.Match = teamVsMatch;
        }
    }
}
