using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class Core
    {
        public event Action<Match> NewMatchAdded;

        public IReadOnlyList<Match> Matches => matches;
        public IEnumerable<Team> Teams { get; }

        private readonly List<Match> matches = new List<Match>();

        public Core(IEnumerable<Team> teams) => Teams = teams;

        public void AddNewMatch(Match match)
        {
            matches.Add(match);
            NewMatchAdded?.Invoke(match);
        }
    }
}
