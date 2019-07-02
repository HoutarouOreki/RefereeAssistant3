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
        public IEnumerable<string> Users { get; private set; } = new List<string>();

        public event Action<IrcMessage> NewMessage;
        public event Action<IEnumerable<string>> UsersUpdated;

        public IrcChannel(string serverName, string channelName)
        {
            ServerName = serverName;
            ChannelName = channelName;
        }

        public void AddMessage(IrcMessage message)
        {
            messages.Add(message);
            NewMessage?.Invoke(message);
        }

        public void SetUsers(IEnumerable<string> users)
        {
            Users = users;
            UsersUpdated?.Invoke(users);
        }
    }
}
