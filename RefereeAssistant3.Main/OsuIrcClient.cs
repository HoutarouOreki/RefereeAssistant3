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

        //private static readonly Regex regex_player_line = new Regex(@"^Slot (\d+)\s+(\w+)\s+https:\/\/osu\.ppy\.sh\/u\/(\d+)\s+([a-zA-Z0-9_ ]+)\s+\[Team (\w+)\s*(?:\/ ([\w, ]+))?\]$");
        //private static readonly Regex regex_room_line = new Regex(@"^Room name: ([^,]*), History:");
        //private static readonly Regex regex_map_line = new Regex(@"Beatmap: [^ ]* (.*)");
        //private static readonly Regex regex_switched_line = new Regex(@"^Switched ([a-zA-Z0-9_\- ]+) to the tournament server$");
        private static readonly Regex regex_create_command = new Regex(@"^Created the tournament match https:\/\/osu\.ppy\.sh\/mp\/(\d+).*$");

        private const string bancho_bot = "BanchoBot";

        public event Action<IrcMessage> PrivateMessageReceived;
        public event Action<IrcMessage> ChannelMessageReceived;
        public event Action<UpdateUsersEventArgs> UserListUpdated;

        public List<IrcChannel> Channels = new List<IrcChannel>();

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
            lastRequestedMatch = null;
            match.RoomId = roomId;
            var channelName = match.ChannelName;
            client.JoinChannel(channelName);
            SendLocalMessage(channelName, $"Chat room created successfully ({match.ChannelName})", true);
            LockMatch(match);
            return;
        }

        public IrcChannel GetChannel(string channelName)
        {
            if (Channels.FirstOrDefault(c => c.ServerName == client.Server && c.ChannelName == channelName) == null)
                Channels.Add(new IrcChannel(client.Server, channelName));
            return Channels.First(c => c.ServerName == client.Server && c.ChannelName == channelName);
        }

        private void OnChannelMessage(ChannelMessageEventArgs e)
        {
            var message = new IrcMessage(e.From, e.Channel, e.Message, DateTime.UtcNow);
            var channel = GetChannel(e.Channel);
            if (message.Equals(channel.Messages.LastOrDefault()))
                return;
            channel.AddMessage(message);
            ChannelMessageReceived?.Invoke(message);
        }

        private void OnUserListUpdated(UpdateUsersEventArgs e)
        {
            GetChannel(e.Channel).SetUsers(e.UserList);
            UserListUpdated?.Invoke(e);
        }

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
