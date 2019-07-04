using RefereeAssistant3.Main.Matches;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.IRC.Events
{
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
}
