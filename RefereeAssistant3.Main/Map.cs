namespace RefereeAssistant3.Main
{
    public class Map
    {
        public string MapCode;
        public int MapsetId;
        public int DifficultyId;
        public string DisplayName;
        public Mods AppliedMods;

        public Map(string mapText)
        {
            var mapData = mapText.Split("|||");
            MapCode = mapData[0];
            DifficultyId = int.Parse(mapData[1]);
            switch (mapData[0])
            {
                case "NM":
                    AppliedMods = Mods.None;
                    break;
                case "FM":
                    AppliedMods = Mods.FreeMod;
                    break;
                case "HR":
                    AppliedMods = Mods.HardRock;
                    break;
                case "HD":
                    AppliedMods = Mods.Hidden;
                    break;
                case "DT":
                    AppliedMods = Mods.DoubleTime;
                    break;
            }
        }
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
