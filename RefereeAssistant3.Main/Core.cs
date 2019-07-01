using osu.Framework.Bindables;
using RefereeAssistant3.Main.Online.APIRequests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Core
    {
        public event Action<Match> NewMatchAdded;

        public Bindable<Match> SelectedMatch = new Bindable<Match>();

        public IReadOnlyList<Match> Matches => matches;
        public IEnumerable<Tournament> Tournaments { get; }
        public OsuIrcBot ChatBot { get; }

        private readonly List<Match> matches = new List<Match>();

        public event Action<string> Alert;

        public Core(IEnumerable<Tournament> tournaments)
        {
            Tournaments = tournaments;
            MainConfig.Load();
            ChatBot = new OsuIrcBot();
        }

        public void AddNewMatch(Match match)
        {
            match.TournamentStage.Mappool.DownloadMappoolAsync();
            matches.Add(match);
            NewMatchAdded?.Invoke(match);
        }

        public void PushAlert(string text) => Alert(text);

        public async Task UpdateMatchAsync()
        {
            var sourceMatch = SelectedMatch.Value;
            var req = await new PutMatchUpdate(sourceMatch.GenerateAPIMatch()).RunTask();
            if (req?.Response?.IsSuccessful == true)
                sourceMatch.NotifyAboutUpload();
            else
                PushAlert($"Updating match {sourceMatch.Code} failed with code {req?.Response?.StatusCode}:\n{req?.Response?.Content}");
        }

        public async Task PostMatchAsync()
        {
            var sourceMatch = SelectedMatch.Value;
            var req = await new PostNewMatch(sourceMatch.GenerateAPIMatch()).RunTask();
            if (req?.Response?.IsSuccessful == true)
            {
                sourceMatch.Id = req.Object.Id;
                PushAlert($"Match {req.Object.Code} posted successfully");
                sourceMatch.NotifyAboutUpload();
            }
            else
            {
                sourceMatch.Id = -1;
                PushAlert($"Failed to post match {sourceMatch.Code}, code {req?.Response?.StatusCode}\n{req?.Response?.ErrorMessage}\n{req?.Response?.Content}");
            }
        }
    }
}
