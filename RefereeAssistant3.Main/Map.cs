using Newtonsoft.Json;
using osu.Framework.Graphics.Textures;
using RefereeAssistant3.Main.Online.APIRequests;
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

        [JsonIgnore]
        public Texture Cover;

        public int Length;

        public bool Downloaded;

        public Map(APIMap apiMap) => SetPropertiesFromAPIMap(apiMap);

        public Map(int difficultyId, string mapCode = null)
        {
            DifficultyId = difficultyId;
            MapCode = mapCode;
        }

        public Map() { }

        public async Task DownloadDataAsync()
        {
            if (Downloaded || !DifficultyId.HasValue)
                return;
            var res = await new GetMap(DifficultyId.Value).RunTask();
            if (res.Response.IsSuccessful && res.Object.Length > 0)
            {
                var apiMap = res.Object[0];
                SetPropertiesFromAPIMap(apiMap);
                Downloaded = true;
            }
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
        NoFail = 1,
        Easy = 2,
        TouchDevice = 4,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        Nightcore = 512, // Only set along with DoubleTime. i.e: NC only gives 576
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        Relax2 = 8192,    // Autopilot
        Perfect = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        FadeIn = 1048576,
        Random = 2097152,
        Cinema = 4194304,
        Target = 8388608,
        Key9 = 16777216,
        KeyCoop = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456,
        ScoreV2 = 536870912,
        LastMod = 1073741824,
        KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
        ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
    }
}
