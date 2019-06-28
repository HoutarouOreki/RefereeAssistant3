using osu.Framework.Threading;
using RefereeAssistant3.Main.Online.APIRequests;
using System;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Map
    {
        public string MapCode;
        public int MapsetId;
        public int DifficultyId;
        public string Artist;
        public string Title;
        public string DifficultyName;
        public Mods AppliedMods;

        private bool downloaded;

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

        public async Task DownloadDataAsync(Core core, Action<Map> OnLoaded = null, Scheduler scheduler = null)
        {
            if (downloaded)
                return;
            var res = await new GetMap(DifficultyId, core).RunTask();
            if (res != null && res.Length > 0)
            {
                var apiMap = res[0];
                MapsetId = apiMap.MapsetId;
                DifficultyId = apiMap.Id;
                Artist = apiMap.Artist;
                Title = apiMap.Title;
                DifficultyName = apiMap.DifficultyName;
                downloaded = true;
            }
            if (scheduler != null && OnLoaded != null)
                scheduler.Add(() => OnLoaded?.Invoke(this));
        }

        public override string ToString() => $"{Artist} - {Title} [{DifficultyName}]";
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
