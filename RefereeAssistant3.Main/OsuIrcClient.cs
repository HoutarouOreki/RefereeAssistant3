using RefereeAssistant3.IRC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RefereeAssistant3.Main
{
    public class OsuIrcBot
    {
        private readonly IrcClient client;

        /// <summary>
        /// 1 - slot
        /// 2 - Not Ready / Ready
        /// 3 - PlayerId
        /// 4 - Player username
        /// 5 - Team and mods
        /// </summary>
        private static readonly Regex regex_player_line = new Regex(@"^Slot (\d+)\s+(Not Ready|Ready) https:\/\/osu\.ppy\.sh\/u\/(\d+)\s+([a-zA-Z0-9_ ]+)\s+(\[(?:.*)\])$");

        /// <summary>
        /// 1 - Blue / Red
        /// 2 - Hidden, HardRock
        /// </summary>
        private static readonly Regex regex_team_and_mods = new Regex(@"\[(?:Host \/ )?(?:Team (\w+)\s*(?:\/ )*)?((?:\w+,\s+)?\w+)?\]");

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

        public OsuIrcBot()
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

        private Match lastRequestedMatch;

        public bool CreateRoom(Match match)
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
            var channelName = match.ChannelName;
            client.JoinChannel(channelName);
            SendLocalMessage(channelName, $"Chat room created successfully ({match.ChannelName})", true);
            match.IrcChannel = GetChannel(match);
            LockMatch(match);
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

        public MpRoomIrcChannel GetChannel(Match match)
        {
            if (Channels.OfType<MpRoomIrcChannel>().FirstOrDefault(c => c.Match == match) == null)
                Channels.Add(new MpRoomIrcChannel(client.Server, match));
            return Channels.OfType<MpRoomIrcChannel>().First(c => c.Match == match);
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
        }

        private void OnUserListUpdated(UpdateUsersEventArgs e)
        {
            var c = GetChannel(e.Channel, true);
            if (c == null)
                return;
            c.SetIrcUsers(e.UserList);
            UserListUpdated?.Invoke(e);
        }

        public void SendMessage(IrcChannel channel, string message) => SendMessage(channel.ChannelName, message);

        public void SendMessage(string channel, string message)
        {
            client.SendMessage(channel, message);
            SendLocalMessage(channel, message);
            client.GetChannelUsers($"{channel}");
        }

        public void SendLocalMessage(string channel, string message, bool fromProgram = false) => OnChannelMessage(new ChannelMessageEventArgs(channel, fromProgram ? "Referee Assistant 3" : client.Nick, message));

        public void InvitePlayer(Match match, Player player) => SendMessage(match.ChannelName, $"!mp invite {player.IRCUsername}");
        public void LockMatch(Match match) => SendMessage(match.ChannelName, "!mp lock");
        public void UnlockMatch(Match match) => SendMessage(match.ChannelName, "!mp unlock");
        public void SetSlotAmount(Match match, int amount) => SendMessage(match.ChannelName, $"!mp size {amount}");
        public void SetProperties(Match match, TeamMode teamMode, ScoreMode scoreMode, int slotAmount) => SendMessage(match.ChannelName, $"!mp set {(int)teamMode} {(int)scoreMode} {slotAmount}");
        public void MovePlayer(Match match, Player player, int slot) => SendMessage(match.ChannelName, $"!mp move {player.IRCUsername} {slot}");
        public void GiveHost(Match match, Player player) => SendMessage(match.ChannelName, $"!mp host {player.IRCUsername}");
        public void ClearHost(Match match) => SendMessage(match.ChannelName, "!mp clearhost");
        public void DisplaySettings(Match match) => SendMessage(match.ChannelName, "!mp settings");
        public void StartMatch(Match match, int secondsUntilStart) => SendMessage(match.ChannelName, $"!mp start {secondsUntilStart}");
        public void AbortMatch(Match match) => SendMessage(match.ChannelName, "!mp abort");
        public void SetPlayerTeam(Match match, Player player, TeamColour team) => SendMessage(match.ChannelName, $"!mp team {player.IRCUsername} {team}");
        public void SetMap(Match match, Map map, PlayMode playMode) => SendMessage(match.ChannelName, $"!mp map {map.DifficultyId} {playMode}");
        public void SetMods(Match match, params ModsLetters[] mods) => SendMessage(match.ChannelName, $"!mp mods {string.Join(' ', mods)}");
        public void SetTimer(Match match, int seconds) => SendMessage(match.ChannelName, $"!mp timer {seconds}");
        public void AbortTimer(Match match) => SendMessage(match.ChannelName, $"!mp aborttimer");
        public void KickPlayer(Match match, string username) => SendMessage(match.ChannelName, $"!mp kick {username}");
        public void SetPassword(Match match, string password) => SendMessage(match.ChannelName, $"!mp password {password}");
        public void AddReferees(Match match, params Player[] referees) => SendMessage(match.ChannelName, $"!mp addref {string.Join(' ', referees.Select(r => r.IRCUsername))}");
        public void RemoveReferees(Match match, params Player[] referees) => SendMessage(match.ChannelName, $"!mp removeref {string.Join(' ', referees.Select(r => r.IRCUsername))}");
        public void ListReferees(Match match) => SendMessage(match.ChannelName, "!mp listrefs");
        public void CloseMatch(Match match) => SendMessage(match.ChannelName, "!mp close");

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

    public enum TeamMode
    {
        HeadToHead = 0,
        TagCoop = 1,
        TeamVs = 2,
        TagTeamVs = 3
    }

    public enum ScoreMode
    {
        Score = 0,
        Accuracy = 1,
        Combo = 2,
        ScoreV2 = 3
    }

    public enum TeamColour
    {
        Blue = 1,
        Red = 2
    }

    public enum PlayMode
    {
        osu = 0,
        Taiko = 1,
        CatchTheBeat = 2,
        Mania = 3
    }

    public enum ModsLetters
    {
        None,
        HR,
        DT,
        HD,
        FL,
        Freemod
    }
}
