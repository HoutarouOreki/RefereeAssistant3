using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class Core
    {
        public event Action<Match> NewMatchAdded;

        public static string APIKey;

        public IReadOnlyList<Match> Matches => matches;
        public IEnumerable<Tournament> Tournaments { get; }

        private readonly List<Match> matches = new List<Match>();

        public Core(IEnumerable<Tournament> tournaments) => Tournaments = tournaments;

        public void AddNewMatch(Match match)
        {
            matches.Add(match);
            NewMatchAdded?.Invoke(match);
        }
    }
}
