using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class TournamentStage
    {
        public string TournamentStageName;
        public string RoomName;
        public List<string> MatchProceedings;
        public int ScoreRequiredToWin;
        public Mappool Mappool = new Mappool();

        public TournamentStage(string stageText)
        {
            var metadata = stageText.Split("\n___\n")[0].Split("|||");
            TournamentStageName = metadata[0];
            RoomName = metadata[1];
            MatchProceedings = metadata[2].Split(' ').ToList();
            ScoreRequiredToWin = int.Parse(metadata[3]);
            var mapTexts = stageText.Split("\n___\n")[1].Split('\n');
            foreach (var mapText in mapTexts)
            {
                var map = new Map(mapText);
                switch (map.AppliedMods)
                {
                    case Mods.None:
                        Mappool.NoMod.Add(map);
                        break;
                    case Mods.FreeMod:
                        Mappool.FreeMod.Add(map);
                        break;
                    case Mods.HardRock:
                        Mappool.HardRock.Add(map);
                        break;
                    case Mods.Hidden:
                        Mappool.Hidden.Add(map);
                        break;
                    case Mods.DoubleTime:
                        Mappool.DoubleTime.Add(map);
                        break;
                }
            }
        }

        public override string ToString() => TournamentStageName;
    }
}
