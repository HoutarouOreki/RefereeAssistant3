using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using RefereeAssistant3.IRC;
using RefereeAssistant3.Main;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Visual
{
    public class ChatContainer : Container
    {
        private const float textbox_height = Style.COMPONENTS_HEIGHT;
        private const float user_list_width = 240;
        private readonly TextFlowContainer textFlow;
        private readonly Core core;
        private readonly Container roomCreationContainer;
        private readonly RA3Button newRoomButton;
        private readonly FillFlowContainer<AvatarUsernameLine> usersFlow;
        private readonly BasicScrollContainer messagesScroll;
        private readonly List<Player> downloadedUsers = new List<Player>();

        public ChatContainer(Core core)
        {
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = user_list_width },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Padding = new MarginPadding { Bottom = textbox_height },
                            RelativeSizeAxes = Axes.Both,
                            Child = messagesScroll = new BasicScrollContainer
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

                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = user_list_width,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FrameworkColour.BlueGreenDark,
                        },
                        new BasicScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(8),
                            Child = usersFlow = new FillFlowContainer<AvatarUsernameLine>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new osuTK.Vector2(4),
                            }
                        }
                    }
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
            core.ChatBot.ChannelMessageReceived += OnNewChannelMessageNonUpdateThread;
            core.ChatBot.UserListUpdated += OnUserListUpdatedNonUpdateThread;
        }

        private void OnMatchChanged(ValueChangedEvent<Match> obj)
        {
            textFlow.Text = "";
            usersFlow.Clear();
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
            {
                roomCreationContainer.Hide();
                var channel = core.ChatBot.GetChannel(obj.NewValue.ChannelName);
                foreach (var message in channel.Messages)
                    OnNewChannelMessage(message);
                PopulateUserList(channel.Users);
            }
        }

        private void OnNewChannelMessageNonUpdateThread(IrcMessage message) => Schedule(() => OnNewChannelMessage(message));

        private void OnNewChannelMessage(IrcMessage message)
        {
            var isScrolledToEnd = messagesScroll.IsScrolledToEnd();
            if (message.Channel == core.SelectedMatch.Value.ChannelName)
            {
                roomCreationContainer.Hide();
                textFlow.AddParagraph($@"{message.DateUTC:hh\:mm} ");
                textFlow.AddText(message.Author, ColourUsername);
                textFlow.AddText($": {message.Message}");
            }
            if (isScrolledToEnd)
            {
                messagesScroll.UpdateSubTree();
                messagesScroll.ScrollToEnd();
            }
        }

        private void OnUserListUpdatedNonUpdateThread(UpdateUsersEventArgs obj) => Schedule(() => OnUserListUpdated(obj));

        private void OnUserListUpdated(UpdateUsersEventArgs obj)
        {
            if (obj.Channel == core.SelectedMatch.Value.ChannelName)
                PopulateUserList(obj.UserList);
        }

        private void PopulateUserList(IEnumerable<string> users)
        {
            usersFlow.Clear();
            foreach (var user in users)
            {
                var p = downloadedUsers.Find(dp => dp.IRCUsername == user.Trim('@', '+'));
                var userLine = new AvatarUsernameLine(p ?? new Player { Username = user.Trim('@', '+') }, true, (line, player) =>
                {
                    ColourUsername(line.UsernameText);
                    downloadedUsers.Add(player);
                });
                usersFlow.Add(userLine);
            }
        }

        private void ColourUsername(SpriteText t)
        {
            if (core.SelectedMatch.Value.Team1.Members.Any(m => m.Id.ToString() == t.Text || m.IRCUsername == t.Text || m.Username == t.Text))
                t.Colour = Style.Red;
            else if (core.SelectedMatch.Value.Team2.Members.Any(m => m.Id.ToString() == t.Text || m.IRCUsername == t.Text || m.Username == t.Text))
                t.Colour = Style.Blue;
        }

        private void OnMessageCommit(TextBox textBox, bool newText)
        {
            var text = textBox.Text.ToLowerInvariant();
            var channel = $"#mp_{core.SelectedMatch.Value.RoomId}";
            if (text.Contains("!mp close") && !core.SelectedMatch.Value.IsFinished && !text.Contains("!mp close confirm"))
            {
                core.ChatBot.SendLocalMessage(channel, @"The match is not yet finished. If you're sure you want to close the room, use command ""!mp close confirm""", true);
                return;
            }
            textBox.Text = string.Empty;
            core.ChatBot.SendMessage(channel, text);
        }
    }
}
