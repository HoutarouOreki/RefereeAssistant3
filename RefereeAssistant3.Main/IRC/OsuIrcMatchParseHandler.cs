using RefereeAssistant3.IRC;
using RefereeAssistant3.Main.IRC.Events;
using RefereeAssistant3.Main.Matches;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.IRC
{
    public class OsuIrcMatchParseHandler
    {
        private readonly BanchoIrcManager bot;
        private readonly IReadOnlyList<OsuMatch> matches;
        private const string name = "rA3";

        public OsuIrcMatchParseHandler(Core core)
        {
            matches = core.Matches;
            bot = core.ChatBot;
            bot.AllPlayersReady += OnAllPlayersReady;
            bot.ChangedSlot += OnChangedSlot;
            bot.JoinedInSlot += OnJoinedSlot;
            bot.LeftRoom += OnLeftRoom;
            bot.MapChanged += OnMapChanged;
            bot.MatchLocked += OnMatchLocked;
            bot.MatchSettingsChanged += OnMatchSettingsChanged;
            bot.MatchUnlocked += OnMatchUnlocked;
            bot.ModsChanged += OnModsChanged;
            bot.PlayerStateReceived += OnPlayerStateReceived;
            bot.PlayerTeamChanged += OnPlayerTeamChanged;
            bot.RefereeAdded += OnRefereeAdded;
            bot.RefereeRemoved += OnRefereeRemoved;
            bot.PlayerFinishedPlaying += OnPlayerFinishedPlaying;
            bot.MatchFinished += OnMatchFinished;
        }

        private void OnMatchFinished(MpRoomIrcChannel channel)
        {
            SendDebugMessage(channel, "Match has ended");
            GetMatch(channel).OnMatchFinished();
        }

        private void OnPlayerFinishedPlaying(PlayerFinishedEventArgs obj)
        {
            SendDebugMessage(obj.Channel, $"{obj.Username} finished with score {obj.Score} and {(obj.Passed ? "passed" : "failed")}");
            GetMatch(obj.Channel).SetPlayerScoreOnCurrentMap(obj.Username, obj.Score, obj.Passed);
        }

        private void OnRefereeRemoved(IrcChannel channel, string obj) => SendDebugMessage(channel, $"{obj} removed from referees");
        private void OnRefereeAdded(IrcChannel channel, string obj) => SendDebugMessage(channel, $"{obj} added as a referee");

        private void OnPlayerTeamChanged(TeamChangedEventArgs obj)
        {
            SendDebugMessage(obj.Channel, $"{obj.Username} changed team to {obj.Team}");
            var match = GetMatch(obj.Channel);
            match.GetPlayer(obj.Username).SelectedTeam = obj.Team;
        }

        private void OnPlayerStateReceived(PlayerStateEventArgs obj) => SendDebugMessage(obj.Channel, $"{name}: player state received: {obj.Username} ({obj.PlayerId}) is {(obj.Ready ? "ready" : "not ready")}, is in slot {obj.Slot}, has {obj.Mods.Count} mods ({string.Join(' ', obj.Mods)}), is in team {obj.Team}");

        private void OnModsChanged(ModsChangedEventArgs obj) => SendDebugMessage(obj.Channel, $"mods changed: {string.Join(", ", obj.Mods)}, freemod is {(obj.FreemodEnabled ? "enabled" : "disabled")}");

        private void OnMatchUnlocked(IrcChannel channel) => SendDebugMessage(channel, $"match unlocked");
        private void OnMatchLocked(IrcChannel channel) => SendDebugMessage(channel, $"match locked");
        private void OnLeftRoom(UserSlotEventArgs obj) => SendDebugMessage(obj.Channel, $"{obj.Username} left room (from slot {obj.Slot})");
        private void OnChangedSlot(UserSlotEventArgs obj) => SendDebugMessage(obj.Channel, $"{obj.Username} changed slot to {obj.Slot}");

        private void OnAllPlayersReady(MpRoomIrcChannel channel)
        {
            SendDebugMessage(channel, $"all players are ready");
            bot.DisplaySettings(channel.Match);
        }

        private void OnJoinedSlot(UserSlotEventArgs obj) => SendDebugMessage(obj.Channel, $"{obj.Username} joined into slot {obj.Slot} (team {(obj.Team.HasValue ? obj.Team.ToString() : "unspecified")})");

        private void OnMatchSettingsChanged(MatchSettingsEventArgs obj) => SendDebugMessage(obj.Channel, $"settings changed: {string.Join(", ", new[] { obj.ScoreMode.ToString(), obj.TeamMode.ToString(), obj.SlotAmount.ToString() })}");

        private void OnMapChanged(IrcChannel channel, int difficultyId)
        {
            SendDebugMessage(channel, $"map was set to {difficultyId}");
            GetMatch(channel).SetMapFromId(difficultyId);
        }

        private OsuMatch GetMatch(string channel) => matches.FirstOrDefault(m => m.ChannelName == channel);
        private OsuMatch GetMatch(IrcChannel channel) => GetMatch(channel.ChannelName);

        private void SendDebugMessage(string channel, string message)
        {
            if (MainConfig.IRCDebugMessages)
                bot.SendMessage(channel, $"rA3 debug: {message}");
        }

        private void SendDebugMessage(IrcChannel channel, string message) => SendDebugMessage(channel.ChannelName, message);
    }
}
