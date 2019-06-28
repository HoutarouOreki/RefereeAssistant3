using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osuTK;
using RefereeAssistant3.Main;
using System.IO;

namespace RefereeAssistant3.Visual
{
    public class RefereeAssistant3Visual : Game
    {
        private const float match_list_width = 396;
        private const float match_list_controls_height = 80;
        private readonly string config_path = $"{Utilities.GetBaseDirectory()}/visualConfig.json";
        private readonly Core core;
        private FillFlowContainer<MatchPreviewPanel> matchListDisplayer;
        private Match selectedMatch;
        private MatchVisualManager matchVisualManager;
        private readonly VisualConfig visualConfig;

        new public GameHost Host => base.Host;

        public RefereeAssistant3Visual(Core core)
        {
            this.core = core;
            if (!File.Exists(config_path))
            {
                visualConfig = new VisualConfig();
                SaveVisualConfig();
            }
            else
                visualConfig = JsonConvert.DeserializeObject<VisualConfig>(File.ReadAllText(config_path));
        }

        private void OnWindowStateChanged(object sender, System.EventArgs e)
        {
            visualConfig.WindowState = Host.Window.WindowState;
            SaveVisualConfig();
        }

        private void SaveVisualConfig() => File.WriteAllTextAsync(config_path, JsonConvert.SerializeObject(visualConfig));

        public DependencyContainer DependencyContainer;
        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => DependencyContainer = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override void LoadComplete()
        {
            DependencyContainer.CacheAs(this);

            Host.Window.Title = "Referee Assistant 3";

            Host.Window.WindowState = visualConfig.WindowState != WindowState.Minimized ? visualConfig.WindowState : WindowState.Normal;
            Host.Window.WindowStateChanged += OnWindowStateChanged;

            Resources.AddStore(new DllResourceStore(@"RefereeAssistant3.Resources.dll"));

            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-Hangul"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Basic"));
            Fonts.AddStore(new GlyphStore(Resources, @"Fonts/Noto-CJK-Compatibility"));

            base.LoadComplete();
            // doing this in the initializer throws
            var newMatchOverlay = new NewMatchOverlay(core);
            var settingsOverlay = new SettingsOverlay(core);
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
                            Child = matchVisualManager = new MatchVisualManager()
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
                                    BackgroundColour = FrameworkColour.Green,
                                    Text = "Add new match",
                                    Action = newMatchOverlay.Show
                                },
                                new RA3Button
                                {
                                    RelativeSizeAxes = Axes.X,
                                    BackgroundColour = FrameworkColour.YellowGreen,
                                    Text = "Settings",
                                    Action = settingsOverlay.Show
                                }
                            }
                        },
                        new BorderContainer(1, true, false, false, false)
                    }
                },
                newMatchOverlay,
                settingsOverlay
            };
            core.NewMatchAdded += OnNewMatchAdded;
            newMatchOverlay.Hide();
            settingsOverlay.Hide();
        }

        private void OnNewMatchAdded(Match match)
        {
            var matchPreviewPanel = new MatchPreviewPanel(match)
            {
                Action = () => SelectMatch(match)
            };
            matchListDisplayer.Add(matchPreviewPanel);
            match.Alert += OnMatchAlert;
            SelectMatch(match);
        }

        private void OnMatchAlert(Match source, string text)
        {
            if (source == selectedMatch)
            {
                var alert = new Alert(text);
                Add(alert);
                alert.Show();
            }
        }

        private void SelectMatch(Match match)
        {
            selectedMatch = match;
            foreach (var matchPanel in matchListDisplayer)
            {
                if (matchPanel.Match == selectedMatch)
                    matchPanel.Select();
                else
                    matchPanel.Deselect();
            }
            matchVisualManager.Match = match;
        }
    }
}
