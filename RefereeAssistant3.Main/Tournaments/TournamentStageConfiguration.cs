using RefereeAssistant3.Main.Matches;
using System.Collections.Generic;

namespace RefereeAssistant3.Main.Tournaments
{
    public class TournamentStageConfiguration
    {
        public string TournamentStageName;

        public List<string> MatchProceedings;

        public int ScoreRequiredToWin;

        public bool DoFailedScoresCount;

        public Mappool Mappool = new Mappool();

        public MpRoomSettings RoomSettings;

        public override string ToString() => TournamentStageName;
    }
}
