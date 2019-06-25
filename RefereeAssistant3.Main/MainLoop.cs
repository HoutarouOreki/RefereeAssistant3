using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class MainLoop
    {
        public event Action<Match> NewMatchAdded;

        public IReadOnlyList<Match> Matches => matches;
        public IEnumerable<Team> Teams { get; }

        private readonly List<Match> matches = new List<Match>();

        public MainLoop(IEnumerable<Team> teams) => Teams = teams;

        public void AddNewMatch(Match match)
        {
            matches.Add(match);
            NewMatchAdded?.Invoke(match);
        }
    }
}
