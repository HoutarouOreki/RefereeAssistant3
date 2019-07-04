using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.IRC.Events
{
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
