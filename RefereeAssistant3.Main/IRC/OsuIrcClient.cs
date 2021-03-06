﻿using RefereeAssistant3.IRC;
using RefereeAssistant3.IRC.Events;
using RefereeAssistant3.Main.IRC.Events;
using RefereeAssistant3.Main.Matches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RefereeAssistant3.Main.IRC
{
    public class BanchoIrcManager
    {
        private readonly IrcClient client;

        /// <summary>
        /// 1 - slot
        /// 2 - Not Ready / Ready
        /// 3 - PlayerId
        /// 4 - Player username
        /// 5 - Team and mods
        /// </summary>
        private static readonly Regex regex_player_line = new Regex(@"^Slot (\d+)\s+(Not Ready|Ready)\s+https:\/\/osu\.ppy\.sh\/u\/(\d+)\s+([a-zA-Z0-9_ ]+)\s+(\[(?:.*)\])$");

        /// <summary>
        /// 1 - Blue / Red
        /// 2 - Hidden, HardRock
        /// </summary>
        private static readonly Regex regex_team_and_mods = new Regex(@"\[(?:Host\s*\/\s*)?(?:Team\s*(\w+)\s*(?:\/\s*)*)?((?:\w+,\s+)*\w+)?\]");

        private static readonly Regex regex_map_line = new Regex(@"(?:B|b)eatmap(?: to|(?: changed to)?:) (?:.*)?https:\/\/osu\.ppy\.sh\/b\/(\d+)");

        //private static readonly Regex regex_switched_line = new Regex(@"^Switched ([a-zA-Z0-9_\- ]+) to the tournament server$");
        private static readonly Regex regex_create_command = new Regex(@"^Created the tournament match https:\/\/osu\.ppy\.sh\/mp\/(\d+).*$");

        /// <summary>
        /// 1 - username
        /// 2 - slot
        /// 3 - team
        /// </summary>
        private static readonly Regex player_joined = new Regex(@"^(.*) joined in slot (\d*)(?: for team (.*))?\.$");

        /// <summary>
        /// 1 - username
        /// 2 - toSlot
        /// </summary>
        private static readonly Regex player_changed_slot = new Regex(@"^(.*) moved to slot (\d*)$");
        private static readonly Regex player_left = new Regex(@"^(.*) left the game.$");

        /// <summary>
        /// 1 - username
        /// 2 - Red/Blue
        /// </summary>
        private static readonly Regex player_changed_team = new Regex(@"^(.*) changed to (Red|Blue)$");

        /// <summary>
        /// 1 - slot amount
        /// 2 - vs mode
        /// 3 - scoring mode
        /// </summary>
        private static readonly Regex match_settings_changed_full = new Regex(@"^Changed match settings to (\d*) slots, (.*), (.*)");

        /// <summary>
        /// 1 - team mode
        /// 2 - win condition
        /// </summary>
        private static readonly Regex match_settings_read = new Regex(@"^Team mode: (\w+), Win condition: (\w+)$");

        /// <summary>
        /// 1 - vs mode
        /// 2 - scoring mode
        /// </summary>
        private static readonly Regex match_settings_changed_without_size = new Regex(@"^Changed match settings to (\w*), (.*)");

        private static readonly Regex vs_mode_changed = new Regex(@"^Changed match settings to (\w+)$");

        /// <summary>
        /// 1 - Enabled/Disabled mods
        /// 2 - mods
        /// 3 - enabled/disabled freemod
        /// </summary>
        private static readonly Regex mods_changed = new Regex(@"^(Enabled|Disabled) (.*), (enabled|disabled) FreeMod$");
        private static readonly Regex mods_active = new Regex(@"^Active mods: ((?:\w+(?:, )?)+)");

        /// <summary>
        /// 1 - irc username
        /// 2 - score
        /// 3 - PASSED/FAILED
        /// </summary>
        private static readonly Regex player_finished = new Regex(@"^((?:\w|\s)+)\s+finished\s+playing\s+\(Score:\s+(\d+),\s+(\w+)\)\.$");

        private static readonly Regex match_finished = new Regex(@"^The match has finished!$");
        private static readonly Regex added_referee = new Regex(@"^Added (.*) to the match referees$");
        private static readonly Regex removed_referee = new Regex(@"^Removed (.*) from the match referees$");
        private static readonly Regex unlocked_room = new Regex(@"^Unlocked the match$");
        private static readonly Regex locked_room = new Regex(@"^Locked the match$");
        private static readonly Regex changed_slot_amount = new Regex(@"^Changed match to size (\d*)$");
        private static readonly Regex all_players_ready = new Regex(@"^All players are ready$");

        private const string bancho_bot = "BanchoBot";

        public event Action<IrcMessage> PrivateMessageReceived;
        public event Action<IrcMessage> ChannelMessageReceived;
        public event Action<UpdateUsersEventArgs> UserListUpdated;

        public List<IrcChannel> Channels = new List<IrcChannel>();

        public event Action<UserSlotEventArgs> JoinedInSlot;
        public event Action<UserSlotEventArgs> ChangedSlot;
        public event Action<UserSlotEventArgs> LeftRoom;
        public event Action<MatchSettingsEventArgs> MatchSettingsChanged;
        public event Action<ModsChangedEventArgs> ModsChanged;
        public event Action<PlayerStateEventArgs> PlayerStateReceived;
        public event Action<TeamChangedEventArgs> PlayerTeamChanged;
        public event Action<MpRoomIrcChannel, int> MapChanged;
        public event Action<MpRoomIrcChannel, string> RefereeAdded;
        public event Action<MpRoomIrcChannel, string> RefereeRemoved;
        public event Action<MpRoomIrcChannel> MatchLocked;
        public event Action<MpRoomIrcChannel> MatchUnlocked;
        public event Action<MpRoomIrcChannel> AllPlayersReady;
        public event Action<PlayerFinishedEventArgs> PlayerFinishedPlaying;
        public event Action<MpRoomIrcChannel> MatchFinished;

        public BanchoIrcManager()
        {
            client = new IrcClient("irc.ppy.sh", false)
            {
                Nick = MainConfig.IRCUsername,
                ServerPass = MainConfig.IRCPassword
            };
            client.ChannelMessage += OnChannelMessage;
            client.PrivateMessage += OnPrivateMessage;
            client.UpdateUsers += OnUserListUpdated;
            client.Connect();
        }

        private OsuMatch lastRequestedMatch;

        public bool CreateRoom(OsuMatch match)
        {
            if (lastRequestedMatch != null)
                return false;
            lastRequestedMatch = match;
            client.SendMessage(bancho_bot, $"!mp make {match.RoomName}");
            return true;
        }

        private void OnPrivateMessage(PrivateMessageEventArgs e)
        {
            var matchCreationMatch = regex_create_command.Match(e.Message);
            if (e.From == bancho_bot && matchCreationMatch.Success)
                OnRoomCreated(int.Parse(matchCreationMatch.Groups[1].Value));
            PrivateMessageReceived?.Invoke(new IrcMessage(e.From, null, e.Message, DateTime.UtcNow));
        }

        private void OnRoomCreated(int roomId)
        {
            var match = lastRequestedMatch;
            if (match == null)
                return;
            lastRequestedMatch = null;
            match.RoomId = roomId;
            match.IrcChannel = GetChannel(match);
            var channelName = $"#mp_{roomId}";
            client.JoinChannel(channelName);
            SendLocalMessage(channelName, $"Chat room created successfully ({channelName})", true);
            LockMatch(match);
            SetProperties(match, match.TournamentStage.RoomSettings.TeamMode, match.TournamentStage.RoomSettings.ScoreMode, 8);
            match.IrcChannel = GetChannel(match);
            return;
        }

        public IrcChannel GetChannel(string channelName, bool dontCreate = false)
        {
            if (Channels.FirstOrDefault(c => c.ServerName == client.Server && c.ChannelName == channelName) == null)
            {
                if (dontCreate)
                    return null;
                Channels.Add(new IrcChannel(client.Server, channelName));
            }
            return Channels.OfType<IrcChannel>().First(c => c.ServerName == client.Server && c.ChannelName == channelName);
        }

        public MpRoomIrcChannel GetChannel(OsuMatch match)
        {
            var channelName = $"#mp_{match.RoomId}";
            if (Channels.OfType<MpRoomIrcChannel>().FirstOrDefault(c => c.ChannelName == channelName) == null)
                Channels.Add(new MpRoomIrcChannel(client.Server, match));
            return Channels.OfType<MpRoomIrcChannel>().First(c => c.ChannelName == channelName);
        }

        private void OnChannelMessage(ChannelMessageEventArgs e)
        {
            var message = new IrcMessage(e.From, e.Channel, e.Message, DateTime.UtcNow);
            var channel = GetChannel(e.Channel, true);
            if (message.Equals(channel?.Messages.LastOrDefault()))
                return;
            if (message.Author == bancho_bot)
                ProcessMessage(message);
            channel?.AddMessage(message);
            ChannelMessageReceived?.Invoke(message);
        }

        private void ProcessMessage(IrcMessage message)
        {
            var t = message.Message;
            var channel = GetChannel(message.Channel, true);
            if (!(channel is MpRoomIrcChannel c))
                return;
            #region room operations
            var joined = player_joined.Match(t);
            if (joined.Success)
            {
                var username = joined.Groups[1].Value;
                var slot = int.Parse(joined.Groups[2].Value);
                var team = joined.Groups[3].Value.Equals("red", StringComparison.InvariantCultureIgnoreCase) ? TeamColour.Red : TeamColour.Blue;
                c.AddSlotUser(username, slot);
                JoinedInSlot?.Invoke(new UserSlotEventArgs(c, username, slot, team));
            }
            var changedSlot = player_changed_slot.Match(t);
            if (changedSlot.Success)
            {
                var username = changedSlot.Groups[1].Value;
                var slot = int.Parse(changedSlot.Groups[2].Value);
                c.MoveSlotUser(username, slot);
                ChangedSlot?.Invoke(new UserSlotEventArgs(c, username, slot));
            }
            var leftRoom = player_left.Match(t);
            if (leftRoom.Success)
            {
                var username = leftRoom.Groups[1].Value;
                var slot = c.RemoveSlot(username);
                LeftRoom?.Invoke(new UserSlotEventArgs(c, username, slot));
            }
            var lockedRoom = locked_room.Match(t);
            if (lockedRoom.Success)
                MatchLocked?.Invoke(c);
            var unlockedRoom = unlocked_room.Match(t);
            if (unlockedRoom.Success)
                MatchUnlocked?.Invoke(c);
            #endregion
            #region match settings
            var fullSettings = match_settings_changed_full.Match(t);
            if (fullSettings.Success)
            {
                var slotAmount = int.Parse(fullSettings.Groups[1].Value);
                var vsMode = TeamModeFromString(fullSettings.Groups[2].Value);
                var scoringMode = ScoreModeFromString(fullSettings.Groups[3].Value);
                MatchSettingsChanged?.Invoke(new MatchSettingsEventArgs(c, slotAmount, scoringMode, vsMode));
            }
            var halfSettings = match_settings_changed_without_size.Match(t);
            if (halfSettings.Success)
            {
                var vsMode = TeamModeFromString(halfSettings.Groups[1].Value);
                var scoringMode = ScoreModeFromString(halfSettings.Groups[2].Value);
                MatchSettingsChanged?.Invoke(new MatchSettingsEventArgs(c, null, scoringMode, vsMode));
            }
            var vsModeChange = vs_mode_changed.Match(t);
            if (vsModeChange.Success)
            {
                var vsMode = TeamModeFromString(vsModeChange.Groups[1].Value);
                MatchSettingsChanged?.Invoke(new MatchSettingsEventArgs(c, null, null, vsMode));
            }
            var settingsRead = match_settings_read.Match(t);
            if (settingsRead.Success)
            {
                var vsMode = TeamModeFromString(settingsRead.Groups[1].Value);
                var winCondition = ScoreModeFromString(settingsRead.Groups[2].Value);
                MatchSettingsChanged?.Invoke(new MatchSettingsEventArgs(c, null, winCondition, vsMode));
            }
            var slotAmountChange = changed_slot_amount.Match(t);
            if (slotAmountChange.Success)
                MatchSettingsChanged?.Invoke(new MatchSettingsEventArgs(c, int.Parse(slotAmountChange.Groups[1].Value), null, null));
            var modsChange = mods_changed.Match(t);
            if (modsChange.Success)
            {
                var modsEnabled = modsChange.Groups[1].Value.Equals("Enabled", StringComparison.InvariantCultureIgnoreCase);
                var mods = new List<Mods>();
                if (modsEnabled)
                    mods.AddRange(modsChange.Groups[2].Value.Split(", ").Select(m => ModFromString(m)));
                var freemodEnabled = modsChange.Groups[3].Value.Equals("enabled", StringComparison.InvariantCultureIgnoreCase);
                ModsChanged?.Invoke(new ModsChangedEventArgs(c, mods, freemodEnabled));
            }
            var modsActive = mods_active.Match(t);
            if (modsActive.Success)
            {
                var modStrings = modsActive.Groups[1].Value.Split(", ").ToList();
                var freemodActive = false;
                if (modStrings.Contains("Freemod"))
                {
                    freemodActive = true;
                    modStrings.RemoveAll(s => s == "Freemod");
                }
                ModsChanged?.Invoke(new ModsChangedEventArgs(c, modStrings.Select(m => ModFromString(m)), freemodActive));
            }
            #endregion
            #region player settings
            var playerState = regex_player_line.Match(t);
            if (playerState.Success)
            {
                var slot = int.Parse(playerState.Groups[1].Value);
                var ready = playerState.Groups[2].Value.Equals("Ready", StringComparison.InvariantCultureIgnoreCase);
                var playerId = int.Parse(playerState.Groups[3].Value);
                var username = playerState.Groups[4].Value;
                var teamAndMods = regex_team_and_mods.Match(playerState.Groups[5].Value);
                var team = teamAndMods.Groups[1].Value.Equals("Red", StringComparison.InvariantCultureIgnoreCase) ? TeamColour.Red : TeamColour.Blue;
                var mods = teamAndMods.Groups[2].Value.Length > 0 ? teamAndMods.Groups[2].Value.Split(", ").Select(m => ModFromString(m)).ToList() : new List<Mods>();
                c.Slots[slot].SelectedMods = mods;
                c.Slots[slot].SelectedTeam = team;
                PlayerStateReceived?.Invoke(new PlayerStateEventArgs(c, slot, ready, playerId, username, team, mods));
            }
            var playerTeamChange = player_changed_team.Match(t);
            if (playerTeamChange.Success)
            {
                var username = playerTeamChange.Groups[1].Value;
                var team = playerTeamChange.Groups[2].Value.Equals("Red", StringComparison.InvariantCultureIgnoreCase) ? TeamColour.Red : TeamColour.Blue;
                PlayerTeamChanged?.Invoke(new TeamChangedEventArgs(c, username, team));
            }
            var allPlayersReady = all_players_ready.Match(t);
            if (allPlayersReady.Success)
                AllPlayersReady?.Invoke(c);
            #endregion
            #region referees
            var refereeAdded = added_referee.Match(t);
            if (refereeAdded.Success)
                RefereeAdded?.Invoke(c, refereeAdded.Groups[1].Value);
            var refereeRemoved = removed_referee.Match(t);
            if (refereeRemoved.Success)
                RefereeRemoved?.Invoke(c, refereeRemoved.Groups[1].Value);
            #endregion
            var mapLine = regex_map_line.Match(t);
            if (mapLine.Success)
                MapChanged?.Invoke(c, int.Parse(mapLine.Groups[1].Value));
            var playerScore = player_finished.Match(t);
            if (playerScore.Success)
            {
                var username = playerScore.Groups[1].Value;
                var score = int.Parse(playerScore.Groups[2].Value);
                var passed = playerScore.Groups[3].Value.Equals("passed", StringComparison.InvariantCultureIgnoreCase);
                PlayerFinishedPlaying?.Invoke(new PlayerFinishedEventArgs(c, username, score, passed));
            }
            if (match_finished.IsMatch(t))
                MatchFinished?.Invoke(c);

        }

        private void OnUserListUpdated(UpdateUsersEventArgs e)
        {
            var c = GetChannel(e.Channel, true);
            if (c == null)
                return;
            c.SetIrcUsers(e.UserList);
            UserListUpdated?.Invoke(e);
        }

        public void SendMessage(IrcChannel channel, string message)
        {
            if (channel?.ChannelName != null)
                SendMessage(channel.ChannelName, message);
        }

        public void SendMessage(string channel, string message)
        {
            client.SendMessage(channel, message);
            SendLocalMessage(channel, message);
            client.GetChannelUsers($"{channel}");
        }

        public void SendLocalMessage(string channel, string message, bool fromProgram = false) => OnChannelMessage(new ChannelMessageEventArgs(channel, fromProgram ? "rA3" : client.Nick, message));

        public void InvitePlayer(OsuMatch match, Player player) => SendMessage(match.IrcChannel, $"!mp invite {player.IRCUsername}");
        public void LockMatch(OsuMatch match) => SendMessage(match.IrcChannel, "!mp lock");
        public void UnlockMatch(OsuMatch match) => SendMessage(match.IrcChannel, "!mp unlock");
        public void SetSlotAmount(OsuMatch match, int amount) => SendMessage(match.IrcChannel, $"!mp size {amount}");
        public void SetProperties(OsuMatch match, TeamMode teamMode, ScoreMode scoreMode, int slotAmount) => SendMessage(match.IrcChannel, $"!mp set {(int)teamMode} {(int)scoreMode} {slotAmount}");
        public void MovePlayer(OsuMatch match, Player player, int slot) => SendMessage(match.IrcChannel, $"!mp move {player.IRCUsername} {slot}");
        public void GiveHost(OsuMatch match, Player player) => SendMessage(match.IrcChannel, $"!mp host {player.IRCUsername}");
        public void ClearHost(OsuMatch match) => SendMessage(match.IrcChannel, "!mp clearhost");
        public void DisplaySettings(OsuMatch match) => SendMessage(match.IrcChannel, "!mp settings");
        public void StartMatch(OsuMatch match, int secondsUntilStart) => SendMessage(match.IrcChannel, $"!mp start {secondsUntilStart}");
        public void AbortMatch(OsuMatch match) => SendMessage(match.IrcChannel, "!mp abort");
        public void SetPlayerTeam(OsuMatch match, Player player, TeamColour team) => SendMessage(match.IrcChannel, $"!mp team {player.IRCUsername} {team}");
        public void SetMap(OsuMatch match, Map map, PlayMode playMode) => SendMessage(match.IrcChannel, $"!mp map {map.DifficultyId} {(int)playMode}");
        public void SetMods(OsuMatch match, params ModsLetters[] mods) => SendMessage(match.IrcChannel, $"!mp mods {string.Join(' ', mods)}");
        public void SetTimer(OsuMatch match, int seconds) => SendMessage(match.IrcChannel, $"!mp timer {seconds}");
        public void AbortTimer(OsuMatch match) => SendMessage(match.IrcChannel, $"!mp aborttimer");
        public void KickPlayer(OsuMatch match, string username) => SendMessage(match.IrcChannel, $"!mp kick {username}");
        public void SetPassword(OsuMatch match, string password) => SendMessage(match.IrcChannel, $"!mp password {password}");
        public void AddReferees(OsuMatch match, params Player[] referees) => SendMessage(match.IrcChannel, $"!mp addref {string.Join(' ', referees.Select(r => r.IRCUsername))}");
        public void RemoveReferees(OsuMatch match, params Player[] referees) => SendMessage(match.IrcChannel, $"!mp removeref {string.Join(' ', referees.Select(r => r.IRCUsername))}");
        public void ListReferees(OsuMatch match) => SendMessage(match.IrcChannel, "!mp listrefs");
        public void CloseMatch(OsuMatch match) => SendMessage(match.IrcChannel, "!mp close");

        private TeamMode? TeamModeFromString(string s)
        {
            if (s.Equals("HeadToHead", StringComparison.InvariantCultureIgnoreCase))
                return TeamMode.HeadToHead;
            if (s.Equals("TagCoop", StringComparison.InvariantCultureIgnoreCase))
                return TeamMode.TagCoop;
            if (s.Equals("TeamVs", StringComparison.InvariantCultureIgnoreCase))
                return TeamMode.TeamVs;
            if (s.Equals("TagTeamVs", StringComparison.InvariantCultureIgnoreCase))
                return TeamMode.TeamVs;
            return null;
        }

        private ScoreMode? ScoreModeFromString(string s)
        {
            if (s.Equals("Score", StringComparison.InvariantCultureIgnoreCase))
                return ScoreMode.Score;
            if (s.Equals("Accuracy", StringComparison.InvariantCultureIgnoreCase))
                return ScoreMode.Accuracy;
            if (s.Equals("Combo", StringComparison.InvariantCultureIgnoreCase))
                return ScoreMode.Combo;
            if (s.Equals("ScoreV2", StringComparison.InvariantCultureIgnoreCase))
                return ScoreMode.ScoreV2;
            return null;
        }

        private Mods ModFromString(string s)
        {
            Enum.TryParse(typeof(Mods), s, out var result);
            return (Mods)result;
        }
    }
}
