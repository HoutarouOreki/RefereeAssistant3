using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using RefereeAssistant3.IRC;
using RefereeAssistant3.Main.Matches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.IRC
{
    public class MpRoomIrcChannel : IrcChannel
    {
        public MpRoomIrcChannel(string serverName, OsuMatch osuMatch) : base(serverName, osuMatch.ChannelName) => Match = osuMatch;

        public MpRoomIrcChannel() { }

        public event Action<Dictionary<int, Player>> SlotsUpdated;

        public DateTime CreationTime { get; } = DateTime.UtcNow;

        public DateTime TimeOutTime { get; private set; } = DateTime.UtcNow;

        [JsonIgnore]
        [BsonIgnore]
        public Dictionary<int, Player> Slots { get; private set; } = new Dictionary<int, Player>();

        [JsonIgnore]
        [BsonIgnore]
        public OsuMatch Match { get; }

        public bool AddSlotUser(string username, int slot)
        {
            if (Slots.ContainsKey(slot))
                return false;
            Slots.Add(slot, Match.GetPlayer(username.Trim('~', '@', '+')));
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

        public int MoveSlotUser(string username, int targetSlot)
        {
            if (Slots.ContainsKey(targetSlot) || !Slots.Values.Any(p => p.Equals(username)))
                return -1;
            var sourceSlot = Slots.Where(kv => kv.Value.Equals(username)).First().Key;
            Slots.Add(targetSlot, Slots[sourceSlot]);
            Slots.Remove(sourceSlot);
            SlotsUpdated?.Invoke(Slots);
            return sourceSlot;
        }

        public bool RemoveSlot(int slot)
        {
            if (!Slots.ContainsKey(slot))
                return false;
            Slots.Remove(slot);
            SlotsUpdated?.Invoke(Slots);
            return true;
        }

        public int RemoveSlot(string user)
        {
            var slot = (int?)null;
            foreach (var _slot in Slots.Keys)
            {
                if (Slots.GetValueOrDefault(_slot)?.Equals(user) == true)
                {
                    slot = _slot;
                    break;
                }
            }
            if (!slot.HasValue)
                return -1;
            if (RemoveSlot(slot.Value))
                return slot.Value;
            return -1;
        }

        /// <summary>
        /// Tournament multiplayer rooms time out in 30 minutes after creation or after the last map was finished (unless a map is in progress). Use this function to refresh this time.
        /// </summary>
        public void RefreshTimeOutTime() => TimeOutTime = DateTime.UtcNow + TimeSpan.FromMinutes(30);
    }
}
