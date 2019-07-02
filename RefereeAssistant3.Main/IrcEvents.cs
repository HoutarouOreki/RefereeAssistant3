using RefereeAssistant3.IRC;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{

    public class UserSlotEventArgs : EventArgs
    {
        public UserSlotEventArgs(IrcChannel channel, string username, int slot)
        {
            Channel = channel;
            Username = username;
            Slot = slot;
        }

        public IrcChannel Channel { get; }
        public string Username { get; }
        public int Slot { get; }
    }

    public class MatchSettingsEventArgs : EventArgs
    {
        public int? SlotAmount { get; }
        public ScoreMode? ScoreMode { get; }
        public TeamMode? TeamMode { get; }

        public MatchSettingsEventArgs(int? slotAmount, ScoreMode? scoreMode, TeamMode? teamMode)
        {
            SlotAmount = slotAmount;
            ScoreMode = scoreMode;
            TeamMode = teamMode;
        }
    }

    public class PlayerStateEventArgs : EventArgs
    {
        public int Slot { get; }
        public bool Ready { get; }
        public int PlayerId { get; }
        public string Username { get; }
        public TeamColour Team { get; }
        public List<Mods> Mods { get; }

        public PlayerStateEventArgs(int slot, bool ready, int playerId, string username, TeamColour team, List<Mods> mods)
        {
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
        public string Username { get; }
        public TeamColour Team { get; }

        public TeamChangedEventArgs(string username, TeamColour team)
        {
            Username = username;
            Team = team;
        }
    }

    public class ModsChangedEventArgs : EventArgs
    {
        public ModsChangedEventArgs(IEnumerable<Mods> mods, bool freemodEnabled)
        {
            Mods = mods;
            FreemodEnabled = freemodEnabled;
        }

        public IEnumerable<Mods> Mods { get; }
        public bool FreemodEnabled { get; }
    }
}
