using RefereeAssistant3.IRC;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{

    public class UserSlotEventArgs : EventArgs
    {
        public UserSlotEventArgs(MpRoomIrcChannel channel, string username, int slot, TeamColour? team = null)
        {
            Channel = channel;
            Username = username;
            Slot = slot;
            Team = team;
        }

        public MpRoomIrcChannel Channel { get; }
        public string Username { get; }
        public int Slot { get; }
        public TeamColour? Team { get; }
    }

    public class MatchSettingsEventArgs : EventArgs
    {
        public MpRoomIrcChannel Channel { get; }
        public int? SlotAmount { get; }
        public ScoreMode? ScoreMode { get; }
        public TeamMode? TeamMode { get; }

        public MatchSettingsEventArgs(MpRoomIrcChannel channel, int? slotAmount, ScoreMode? scoreMode, TeamMode? teamMode)
        {
            Channel = channel;
            SlotAmount = slotAmount;
            ScoreMode = scoreMode;
            TeamMode = teamMode;
        }
    }

    public class PlayerStateEventArgs : EventArgs
    {
        public MpRoomIrcChannel Channel { get; }
        public int Slot { get; }
        public bool Ready { get; }
        public int PlayerId { get; }
        public string Username { get; }
        public TeamColour Team { get; }
        public List<Mods> Mods { get; }

        public PlayerStateEventArgs(MpRoomIrcChannel channel, int slot, bool ready, int playerId, string username, TeamColour team, List<Mods> mods)
        {
            Channel = channel;
            Slot = slot;
            Ready = ready;
            PlayerId = playerId;
            Username = username;
            Team = team;
            Mods = mods;
        }
    }

    public class TeamChangedEventArgs : EventArgs
    {
        public MpRoomIrcChannel Channel { get; }
        public string Username { get; }
        public TeamColour Team { get; }

        public TeamChangedEventArgs(MpRoomIrcChannel channel, string username, TeamColour team)
        {
            Channel = channel;
            Username = username;
            Team = team;
        }
    }

    public class ModsChangedEventArgs : EventArgs
    {
        public ModsChangedEventArgs(MpRoomIrcChannel channel, IEnumerable<Mods> mods, bool freemodEnabled)
        {
            Channel = channel;
            Mods = mods;
            FreemodEnabled = freemodEnabled;
        }

        public MpRoomIrcChannel Channel { get; }
        public IEnumerable<Mods> Mods { get; }
        public bool FreemodEnabled { get; }
    }
}
