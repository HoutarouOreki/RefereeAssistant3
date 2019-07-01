using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using RefereeAssistant3.IRC;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class ChatContainer : Container
    {
        private const float tabs_container_height = 50;
        private const float textbox_height = Style.COMPONENTS_HEIGHT;
        private readonly TextFlowContainer textFlow;
        private readonly Core core;
        private readonly Container roomCreationContainer;
        private readonly RA3Button newRoomButton;

        public ChatContainer(Core core)
        {
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = tabs_container_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FrameworkColour.BlueGreenDark
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal
                        }
                    }
                },
                new Container
                {
                    Padding = new MarginPadding { Top = tabs_container_height, Bottom = textbox_height },
                    RelativeSizeAxes = Axes.Both,
                    Child = new BasicScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = textFlow = new TextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        }
                    }
                },
                new BasicTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = textbox_height,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    ReleaseFocusOnCommit = false,
                    OnCommit = OnMessageCommit
                },
                roomCreationContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FrameworkColour.BlueGreenDark
                        },
                        newRoomButton = new RA3Button
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Create match room",
                            Size = new osuTK.Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                            BackgroundColour = FrameworkColour.Blue
                        }
                    }
                }
            };
            this.core = core;
            core.SelectedMatch.BindValueChanged(OnMatchChanged);
            core.ChatBot.ChannelMessageReceived += OnNewChannelMessage;
        }

        private void OnMatchChanged(ValueChangedEvent<Match> obj)
        {
            textFlow.Text = "";
            if (obj.NewValue.RoomId == null)
            {
                newRoomButton.Action = () =>
                {
                    newRoomButton.Action = null;
                    core.ChatBot.CreateRoom(core.SelectedMatch.Value);
                };
                roomCreationContainer.Show();
            }
            else
                roomCreationContainer.Hide();
        }

        private void OnNewChannelMessage(IrcMessage message)
        {
            Schedule(() =>
            {
                if (message.Channel == $"#mp_{core.SelectedMatch.Value.RoomId}")
                {
                    roomCreationContainer.Hide();
                    textFlow.AddParagraph($@"{message.DateUTC:hh\:mm} | {message.Author}: ", t => t.Font = new FontUsage("OpenSans-Bold"));
                    textFlow.AddText(message.Message);
                }
            });
        }

        private void OnMessageCommit(TextBox textBox, bool newText)
        {
            var text = textBox.Text.ToLowerInvariant();
            var channel = $"#mp_{core.SelectedMatch.Value.RoomId}";
            if (text.Contains("!mp close") && !core.SelectedMatch.Value.IsFinished && !text.Contains("!mp close confirm"))
            {
                core.ChatBot.SendLocalMessage(channel, @"The match is not yet finished. If you're sure you want to close the room, use command ""!mp close confirm""");
                return;
            }
            core.ChatBot.SendMessage(channel, text);
        }
    }
}
