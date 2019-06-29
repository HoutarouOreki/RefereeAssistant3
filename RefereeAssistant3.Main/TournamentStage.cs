using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class TournamentStage
    {
        [JsonRequired]
        public string TournamentStageName;

        public string RoomName;

        [JsonRequired]
        public List<string> MatchProceedings;

        [JsonRequired]
        public int ScoreRequiredToWin;

        public Mappool Mappool = new Mappool();

        public TournamentStage(string stageText)
        {
            var metadata = stageText.Split("\n___\n")[0].Split("|||");
            TournamentStageName = metadata[0];
            RoomName = metadata[1];
            MatchProceedings = metadata[2].Split(' ').ToList();
            ScoreRequiredToWin = int.Parse(metadata[3]);
            var mapTexts = stageText.Split("\n___\n").ElementAtOrDefault(1)?.Split('\n') ?? new string[0];
            foreach (var mapText in mapTexts)
            {
                var map = new Map(mapText);
            }
        }

        public TournamentStage() { }

        public override string ToString() => TournamentStageName;
    }
}
