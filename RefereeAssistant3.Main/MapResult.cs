using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class MapResult
    {
        public int DifficultyId;

        /// <summary>
        /// Key = player's id. Value = player's score.
        /// </summary>
        public Dictionary<int, int> PlayerScores;
    }
}
