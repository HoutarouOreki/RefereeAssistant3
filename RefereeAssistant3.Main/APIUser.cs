using Newtonsoft.Json;

namespace RefereeAssistant3.Main
{
    public class APIUser
    {
        [JsonProperty("user_id")]
        public int? Id;

        [JsonProperty("username")]
        public string Username;
    }
}
