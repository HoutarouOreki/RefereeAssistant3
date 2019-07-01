using System;

namespace RefereeAssistant3.IRC
{
    public class IrcMessage
    {
        public DateTime DateUTC;
        public string Author;
        public string Channel;
        public string Message;

        public IrcMessage(string author, string channel, string content, DateTime receiveDateTimeUTC)
        {
            DateUTC = receiveDateTimeUTC;
            Author = author;
            Channel = channel;
            Message = content;
        }
    }
}
