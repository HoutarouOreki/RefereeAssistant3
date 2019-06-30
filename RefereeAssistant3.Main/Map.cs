using Newtonsoft.Json;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using RefereeAssistant3.Main.Online.APIRequests;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Map
    {
        public string MapCode;
        public int? MapsetId;

        [JsonRequired]
        public int? DifficultyId;

        public string Artist;
        public string Title;
        public string DifficultyName;
        public Texture Cover;
        public int Length;

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

        public Texture DownloadCover(TextureStore textures)
        {
            var coverCachePath = $"{Utilities.GetBaseDirectory()}/cache/maps/{MapsetId}.jpg";
            if (Cover == null && MapsetId != null)
            {
                if (!File.Exists(coverCachePath))
                {
                    var coverReq = textures.GetStream($"https://assets.ppy.sh/beatmaps/{MapsetId}/covers/cover.jpg");
                    if (coverReq == null)
                        return null;
                    using (var stream = new FileStream(coverCachePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        var img = SixLabors.ImageSharp.Image.Load(coverReq);
                        SixLabors.ImageSharp.ImageExtensions.SaveAsJpeg(img, stream);
                    }
                }
                using (var stream = new FileStream(coverCachePath, FileMode.Open, FileAccess.Read))
                { return Cover = Texture.FromStream(stream); }
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
            Length = apiMap.Length;
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
