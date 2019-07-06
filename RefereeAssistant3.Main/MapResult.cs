using System.Collections.Generic;

namespace RefereeAssistant3.Main
{
    public class PlayerMapResult
    {
        public int DifficultyId;
        public int Score;
        public IEnumerable<Mods> SelectedMods;
        public bool Passed;

        public PlayerMapResult(int difficultyId, bool passed, int score, IEnumerable<Mods> selectedMods = null)
        {
            DifficultyId = difficultyId;
            Passed = passed;
            Score = score;
            SelectedMods = selectedMods ?? new List<Mods>();
        }
    }
}
