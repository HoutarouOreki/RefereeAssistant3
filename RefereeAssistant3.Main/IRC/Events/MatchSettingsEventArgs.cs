using RefereeAssistant3.Main.Matches;
using System;

namespace RefereeAssistant3.Main.IRC.Events
{
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
}
