using System;

namespace RefereeAssistant3.IRC.Events
{
    public class NoticeMessageEventArgs : EventArgs
    {
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public NoticeMessageEventArgs(string from, string message)
        {
            From = from;
            Message = message;
        }
    }
}
