using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [JsonIgnore]
        public Dictionary<int, string> Slots { get; private set; } = new Dictionary<int, string>();

        public event Action<IrcMessage> NewMessage;
        public event Action<IEnumerable<string>> IrcUsersUpdated;
        public event Action<Dictionary<int, string>> SlotsUpdated;

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

        public bool AddSlotUser(string user, int slot)
        {
            if (Slots.ContainsKey(slot))
                return false;
            Slots.Add(slot, user);
            SlotsUpdated?.Invoke(Slots);
            return true;
        }

        public bool MoveSlotUser(int sourceSlot, int targetSlot)
        {
            if (!Slots.ContainsKey(sourceSlot) || Slots.ContainsKey(targetSlot))
                return false;
            Slots.Add(targetSlot, Slots[sourceSlot]);
            Slots.Remove(sourceSlot);
            SlotsUpdated?.Invoke(Slots);
            return true;
        }

        public bool RemoveSlot(int slot)
        {
            if (!Slots.ContainsKey(slot))
                return false;
            Slots.Remove(slot);
            SlotsUpdated?.Invoke(Slots);
            return true;
        }

        public bool RemoveSlot(string user)
        {
            var slot = (int?)null;
            foreach (var _slot in Slots.Keys)
            {
                if (Slots[_slot] == user)
                {
                    slot = _slot;
                    break;
                }
            }
            if (!slot.HasValue)
                return false;
            return RemoveSlot(slot.Value);
        }
    }
}
