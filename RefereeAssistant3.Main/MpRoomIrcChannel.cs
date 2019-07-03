using Newtonsoft.Json;
using RefereeAssistant3.IRC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class MpRoomIrcChannel : IrcChannel
    {
        public MpRoomIrcChannel(string serverName, Match match) : base(serverName, match.ChannelName) => Match = match;

        [JsonIgnore]
        public readonly Match Match;

        public event Action<Dictionary<int, Player>> SlotsUpdated;

        [JsonIgnore]
        public Dictionary<int, Player> Slots { get; private set; } = new Dictionary<int, Player>();

        public bool AddSlotUser(string username, int slot)
        {
            if (Slots.ContainsKey(slot))
                return false;
            var player = Match.GetPlayer(username) ?? new Player(username);
            Slots.Add(slot, player);
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
                if (Slots[_slot].Equals(user))
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
    }
}
