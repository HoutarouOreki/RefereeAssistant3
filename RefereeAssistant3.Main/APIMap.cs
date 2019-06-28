using Newtonsoft.Json;

namespace RefereeAssistant3.Main
{
    public class APIMap
    {
        [JsonProperty("beatmap_id")]
        public int Id;

        [JsonProperty("beatmapset_id")]
        public int MapsetId;

        [JsonProperty("artist")]
        public string Artist;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("version")]
        public string DifficultyName;

        [JsonProperty("total_length")]
        public int Length;
    }
}
