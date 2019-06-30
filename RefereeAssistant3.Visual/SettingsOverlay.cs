using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class SettingsOverlay : RA3OverlayContainer
    {
        private BasicTextBox apiKeyTextBox;
        private BasicScrollContainer scroll;
        private FillFlowContainer warningContainer;
        private BasicTextBox ircUsernameTextBox;
        private BasicTextBox ircPasswordTextBox;

        [BackgroundDependencyLoader]
        private void Load(RefereeAssistant3Visual app)
        {
            Children = new Drawable[]
            {
                warningContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(Style.SPACING),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "WARNING",
                            Font = new FontUsage(null, 30)
                        },
                        new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            Width = 500,
                            Text = "This screen contains sensitive data, like your IRC password and API Key.\n" +
                            "Don't show it to anyone."
                        },
                        new RA3Button
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                            Text = "Proceed",
                            Action = () =>
                            {
                                scroll.Show();
                                warningContainer.Hide();
                            }
                        },
                        new RA3Button
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                            Text = "Go back",
                            BackgroundColour = FrameworkColour.YellowDark,
                            Action = () => Hide()
                        }
                    }
                },
                scroll = new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = (Style.COMPONENTS_WIDTH * 2) + Style.SPACING,
                    Alpha = 0,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(Style.SPACING),
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                Width = Style.COMPONENTS_WIDTH,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(Style.SPACING),
                                Children = new Drawable[]
                                {
                                    new Label("API Key:"),
                                    apiKeyTextBox = new BasicTextBox
                                    {
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        PlaceholderText = "API Key"
                                    },
                                    new Label("IRC Username:"),
                                    ircUsernameTextBox = new BasicTextBox
                                    {
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        PlaceholderText = "IRC Username"
                                    },
                                    new Label("IRC Password:"),
                                    ircPasswordTextBox = new BasicTextBox
                                    {
                                        Width = Style.COMPONENTS_WIDTH,
                                        Height = Style.COMPONENTS_HEIGHT,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        PlaceholderText = "IRC Password"
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                Width = Style.COMPONENTS_WIDTH,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(Style.SPACING),
                                Children = new Drawable[]
                                {
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
                                        BackgroundColour = FrameworkColour.Blue,
                                        Action = () => app.Host.OpenUrlExternally("https://osu.ppy.sh/p/irc"),
                                        Text = "Get your IRC details here"
                                    },
                                    new TextFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Text = "Note: if the link to the API Key redirects you " +
                                        "to the forums, wait for a minute and try accessing it again."
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
                    }
                }
            };
        }

        private void ApplySettings()
        {
            MainConfig.APIKey = apiKeyTextBox.Text;
            MainConfig.IRCUsername = ircUsernameTextBox.Text;
            MainConfig.IRCPassword = ircPasswordTextBox.Text;
            MainConfig.Save();
            Hide();
        }

        protected override void PopIn()
        {
            scroll.Hide();
            warningContainer.Show();
            apiKeyTextBox.Text = MainConfig.APIKey;
            ircUsernameTextBox.Text = MainConfig.IRCUsername;
            ircPasswordTextBox.Text = MainConfig.IRCPassword;
            base.PopIn();
        }

        private class Label : SpriteText
        {
            public Label(string text)
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                Text = text;
                Margin = new MarginPadding { Bottom = -Style.SPACING };
            }
        }
    }
}
