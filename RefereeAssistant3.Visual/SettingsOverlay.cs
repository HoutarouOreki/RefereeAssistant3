using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class SettingsOverlay : RA3OverlayContainer
    {
        private BasicTextBox apiKeyTextBox;
        private readonly Core core;

        public SettingsOverlay(Core core) => this.core = core;

        [BackgroundDependencyLoader]
        private void Load(RefereeAssistant3Visual app)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.GreenDarker,
                    Alpha = 0.9f
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(Style.SPACING),
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = "Api Key:"
                            },
                            apiKeyTextBox = new BasicTextBox
                            {
                                Width = Style.COMPONENTS_WIDTH,
                                Height = Style.COMPONENTS_HEIGHT,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                PlaceholderText = "API Key",
                                Text = core.Config.ApiKey
                            },
                            new RA3Button
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = Style.COMPONENTS_WIDTH,
                                BackgroundColour = FrameworkColour.Blue,
                                Action = () => app.Host.OpenUrlExternally("https://osu.ppy.sh/p/api"),
                                Text = "Get your API Key here"
                            },
                            new RA3Button
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = Style.COMPONENTS_WIDTH,
                                BackgroundColour = FrameworkColour.Green,
                                Action = ApplySettings,
                                Text = "Apply settings"
                            }
                        }
                    }
                }
            };
        }

        private void ApplySettings()
        {
            core.Config.ApiKey = apiKeyTextBox.Text;
            core.SaveConfig();
            Hide();
        }
    }
}
