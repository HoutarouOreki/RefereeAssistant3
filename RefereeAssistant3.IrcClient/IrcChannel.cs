using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.IRC
{
    public class IrcChannel
    {
        public string ServerName { get; }
        public string ChannelName { get; }
        private readonly List<IrcMessage> messages = new List<IrcMessage>();
        public IReadOnlyList<IrcMessage> Messages => messages;

        [JsonIgnore]
        public IEnumerable<string> IrcUsers { get; private set; } = new List<string>();

        public event Action<IrcMessage> NewMessage;
        public event Action<IEnumerable<string>> IrcUsersUpdated;

        public IrcChannel(string serverName, string channelName)
        {
            ServerName = serverName;
            ChannelName = channelName;
        }

        public IrcChannel() { }

        public void AddMessage(IrcMessage message)
        {
            messages.Add(message);
            NewMessage?.Invoke(message);
        }

        public void SetIrcUsers(IEnumerable<string> users)
        {
            var list = new List<string>();
            foreach (var user in users)
            {
                if ("~&@%+".Contains(user[0]))
                    list.Add(user.Trim('~', '&', '@', '%', '+'));
            }
            IrcUsersUpdated?.Invoke(users);
        }
    }
}
