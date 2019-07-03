﻿using RefereeAssistant3.IRC;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class OsuIrcMatchParseHandler
    {
        private readonly OsuIrcBot bot;
        private readonly IReadOnlyList<Match> matches;
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
        }

        private void OnRefereeRemoved(IrcChannel channel, string obj) => bot.SendMessage(channel, $"{name}: {obj} removed from referees");
        private void OnRefereeAdded(IrcChannel channel, string obj) => bot.SendMessage(channel, $"{name}: {obj} added as a referee");

        private void OnPlayerTeamChanged(TeamChangedEventArgs obj)
        {
            bot.SendMessage(obj.Channel, $"{name}: {obj.Username} changed team to {obj.Team}");
            var match = GetMatch(obj.Channel);
            match.GetPlayer(obj.Username).SelectedTeam = obj.Team;
        }

        private void OnPlayerStateReceived(PlayerStateEventArgs obj) => bot.SendMessage(obj.Channel, $"{name}: player state received: {obj.Username} ({obj.PlayerId}) is {(obj.Ready ? "ready" : "not ready")}, is in slot {obj.Slot}, has {obj.Mods.Count} mods ({string.Join(' ', obj.Mods)}), is in team {obj.Team}");
        private void OnModsChanged(ModsChangedEventArgs obj) => bot.SendMessage(obj.Channel, $"{name}: mods changed: {string.Join(", ", obj.Mods)}, freemod is {(obj.FreemodEnabled ? "enabled" : "disabled")}");
        private void OnMatchUnlocked(IrcChannel channel) => bot.SendMessage(channel, $"{name}: match unlocked");
        private void OnMatchSettingsChanged(MatchSettingsEventArgs obj) => bot.SendMessage(obj.Channel, $"{name}: settings changed: {string.Join(", ", new[] { obj.ScoreMode.ToString(), obj.TeamMode.ToString(), obj.SlotAmount.ToString() })}");
        private void OnMatchLocked(IrcChannel channel) => bot.SendMessage(channel, $"{name}: match locked");
        private void OnLeftRoom(UserSlotEventArgs obj) => bot.SendMessage(obj.Channel, $"{name}: {obj.Username} left room (from slot {obj.Slot})");
        private void OnJoinedSlot(UserSlotEventArgs obj) => bot.SendMessage(obj.Channel, $"{name}: {obj.Username} joined into slot {obj.Slot}");
        private void OnChangedSlot(UserSlotEventArgs obj) => bot.SendMessage(obj.Channel, $"{name}: {obj.Username} changed slot to {obj.Slot}");
        private void OnAllPlayersReady(IrcChannel channel) => bot.SendMessage(channel, $"{name}: all players are ready");

        private void OnMapChanged(IrcChannel channel, int difficultyId)
        {
            bot.SendMessage(channel, $"{name}: map was set to {difficultyId}");
            GetMatch(channel).SelectMapFromId(difficultyId);
        }

        private Match GetMatch(string channel) => matches.FirstOrDefault(m => m.ChannelName == channel);
        private Match GetMatch(IrcChannel channel) => GetMatch(channel.ChannelName);
    }
}
