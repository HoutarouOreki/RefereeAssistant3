using System;

namespace RefereeAssistant3.IRC.Events
{
    public class UserLeftEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string User { get; internal set; }

        public UserLeftEventArgs(string channel, string user)
        {
            Channel = channel;
            User = user;
        }
    }
}
