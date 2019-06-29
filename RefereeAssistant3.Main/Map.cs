using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using RefereeAssistant3.Main.Online.APIRequests;
using System;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Map
    {
        public string MapCode;
        public int? MapsetId;
        public int? DifficultyId;
        public string Artist;
        public string Title;
        public string DifficultyName;
        public Texture Cover;

        private bool downloaded;

        public Map(string mapText)
        {
            var mapData = mapText.Split("|||");
            MapCode = mapData[0];
            DifficultyId = int.Parse(mapData[1]);
        }

        public Map(APIMap apiMap) => SetPropertiesFromAPIMap(apiMap);

        public Map(int difficultyId) => DifficultyId = difficultyId;

        public Map() { }

        public async Task DownloadDataAsync(Core core, Action<Map> OnLoaded = null, Scheduler scheduler = null)
        {
            if (downloaded || !DifficultyId.HasValue)
                return;
            var res = await new GetMap(DifficultyId.Value, core).RunTask();
            if (res != null && res.Length > 0)
            {
                var apiMap = res[0];
                SetPropertiesFromAPIMap(apiMap);
                downloaded = true;
            }

            if (scheduler != null && OnLoaded != null)
                scheduler.Add(() => OnLoaded?.Invoke(this));
        }

        public async Task<Texture> DownloadCoverAsync(TextureStore textures)
        {
            if (Cover == null && MapsetId != null)
            {
                var coverReq = textures.GetAsync($"https://assets.ppy.sh/beatmaps/{MapsetId}/covers/cover.jpg");
                return Cover = await coverReq;
            }
            else if (Cover != null)
                return Cover;
            else
                return null;
        }

        public Texture DownloadCover(TextureStore textures)
        {
            if (Cover == null && MapsetId != null)
            {
                return Cover = textures.Get($"https://assets.ppy.sh/beatmaps/{MapsetId}/covers/cover.jpg");
            }
            else if (Cover != null)
                return Cover;
            else
                return null;
        }

        private void SetPropertiesFromAPIMap(APIMap apiMap)
        {
            MapsetId = apiMap.MapsetId;
            DifficultyId = apiMap.Id;
            Artist = apiMap.Artist;
            Title = apiMap.Title;
            DifficultyName = apiMap.DifficultyName;
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
