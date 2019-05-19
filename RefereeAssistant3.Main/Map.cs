namespace RefereeAssistant3.Main
{
    public class Map
    {
        public int MapsetId;
        public int DifficultyId;
        public Mods AppliedMods;
    }

    public enum Mods
    {
        None = 0,
        FreeMod = 1,
        HardRock = 2,
        Hidden = 3,
        DoubleTime = 4,
    }
}
