using RefereeAssistant3.Main.Matches;
using System;

namespace RefereeAssistant3.Main.IRC.Events
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
}
