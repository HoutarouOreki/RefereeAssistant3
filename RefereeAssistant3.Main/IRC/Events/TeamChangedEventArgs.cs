using RefereeAssistant3.Main.Matches;
using System;

namespace RefereeAssistant3.Main.IRC.Events
{
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
}
