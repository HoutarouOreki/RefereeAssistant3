﻿using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using RefereeAssistant3.IRC;
using RefereeAssistant3.IRC.Events;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Matches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Visual.UI
{
    public class ChatContainer : Container
    {
        private const float user_list_width = 240;
        private readonly FillFlowContainer<MessageLine> messagesFlow;
        private readonly Core core;
        private readonly Container roomCreationContainer;
        private readonly RA3Button newRoomButton;
        private readonly FillFlowContainer<SlotLine> slotsFlow;
        private readonly BasicScrollContainer messagesScroll;
        private readonly List<Player> downloadedUsers = new List<Player>();
        private readonly SpriteText matchTimeOutText;
        private readonly FillFlowContainer<AvatarUsernameLine> ircUsersFlow;

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
                            Padding = new MarginPadding { Bottom = Style.COMPONENTS_HEIGHT },
                            RelativeSizeAxes = Axes.Both,
                            Child = messagesScroll = new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = messagesFlow = new FillFlowContainer<MessageLine>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y
                                }
                            }
                        },
                        new BasicTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = Style.COMPONENTS_HEIGHT,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            ReleaseFocusOnCommit = false,
                            OnCommit = OnMessageCommit
                        }
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
                            Child = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    slotsFlow = new FillFlowContainer<SlotLine>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new osuTK.Vector2(4)
                                    },
                                    new Box { RelativeSizeAxes = Axes.X },
                                    ircUsersFlow = new FillFlowContainer<AvatarUsernameLine>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new osuTK.Vector2(4)
                                    }
                                }
                            }
                        },
                        matchTimeOutText = new SpriteText
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Font = new FontUsage("OpenSans-Bold", 14),
                            AllowMultiline = true,
                            Margin = new MarginPadding { Horizontal = 4 },
                            Alpha = 0.5f
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

        private void OnMatchChanged(ValueChangedEvent<OsuMatch> obj)
        {
            messagesFlow.Clear();
            slotsFlow.Clear();
            ircUsersFlow.Clear();
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
                var channel = core.ChatBot.GetChannel(obj.NewValue);
                foreach (var message in channel.Messages)
                    OnNewChannelMessage(message);
                PopulateUserList(channel.IrcUsers);
            }
        }

        private void OnNewChannelMessageNonUpdateThread(IrcMessage message) => Schedule(() => OnNewChannelMessage(message));

        private void OnNewChannelMessage(IrcMessage message)
        {
            var isScrolledToEnd = messagesScroll.IsScrolledToEnd();
            if (message.Channel == core.SelectedMatch.Value.IrcChannel.ChannelName)
            {
                roomCreationContainer.Hide();
                var authorText = (SpriteText)null;
                messagesFlow.Add(new MessageLine(message, t => authorText = t));
                authorText.Text = GetSavedPlayer(message.Author)?.Username ?? message.Author;
                ColourUsername(authorText);
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
            if (obj.Channel == core.SelectedMatch.Value.IrcChannel.ChannelName)
                PopulateUserList(obj.UserList);
        }

        private void PopulateUserList(IEnumerable<string> users)
        {
            ircUsersFlow.Clear();
            foreach (var user in users)
            {
                var userLine = new AvatarUsernameLine(GetSavedPlayer(user), true, (line, player) =>
                {
                    ColourUsername(line.UsernameText);
                    if (!downloadedUsers.Contains(player))
                        downloadedUsers.Add(player);
                });
                ircUsersFlow.Add(userLine);
            }

            slotsFlow.Clear();
            var slots = core.SelectedMatch.Value.IrcChannel.Slots;
            if (slots.Count == 0)
                return;
            for (var i = 1; i <= slots.Keys.Max(); i++)
            {
                if (slots.ContainsKey(i))
                {
                    var player = slots[i];
                    var userLine = new SlotLine(i, player, (line, p) =>
                    {
                        ColourUsername(line.UsernameText);
                        if (!downloadedUsers.Contains(p))
                            downloadedUsers.Add(p);
                    });
                    slotsFlow.Add(userLine);
                }
                else
                    slotsFlow.Add(new SlotLine(i, null));
            }
        }

        private void ColourUsername(SpriteText t)
        {
            if (core.SelectedMatch.Value is OsuTeamVsMatch teamVsMatch)
            {
                if (teamVsMatch.Team1.Members.Any(m => m.Equals(t.Text.Text.Original)))
                    t.Colour = Style.Red;
                else if (teamVsMatch.Team2.Members.Any(m => m.Equals(t.Text.Text.Original)))
                    t.Colour = Style.Blue;
                else
                    t.Colour = FrameworkColour.YellowDark;
            }
        }

        private Player GetSavedPlayer(string username) => core.SelectedMatch.Value.GetPlayer(username.Trim('@', '+', '~')) ?? downloadedUsers.Find(dp => dp.IRCUsername == username.Trim('@', '+', '~'));

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

        protected override void Update()
        {
            if (core.SelectedMatch.Value?.IrcChannel != null)
                matchTimeOutText.Text = $@"this multiplayer room will time out in {core.SelectedMatch.Value.IrcChannel.TimeOutTime - DateTime.UtcNow:mm\:ss}";
            else
                matchTimeOutText.Text = string.Empty;
            base.Update();
        }

        private class MessageLine : Container
        {
            private const float time_author_width = 200;

            private readonly SpriteText author;

            public MessageLine(IrcMessage message, Action<SpriteText> authorTextCallback = null)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    new SpriteText { Text = $@"{message.DateUTC:hh\:mm}" },
                    author = new SpriteText { Text = message.Author, X = 50 },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = time_author_width },
                        Child = new TextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = message.Message
                        }
                    }
                };
                authorTextCallback?.Invoke(author);
            }
        }
    }
}
