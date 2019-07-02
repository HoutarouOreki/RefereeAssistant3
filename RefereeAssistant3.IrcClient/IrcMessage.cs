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

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is IrcMessage other))
                return false;
            if (DateUTC != other.DateUTC)
                return false;
            if (Author != other.Author)
                return false;
            if (Channel != other.Channel)
                return false;
            if (Message != other.Channel)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hash = 21;
            hash *= 4 + DateUTC.GetHashCode();
            hash *= 3 + Author.GetHashCode();
            hash *= 2 + Channel.GetHashCode();
            hash += Message.GetHashCode();
            return hash;
        }
    }
}
