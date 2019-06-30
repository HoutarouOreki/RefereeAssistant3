using RestSharp;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public class GetMap : APIRequest<APIMap[]>
    {
        public override Method Method => Method.GET;

        public override string Title => "Get map";

        protected override string BaseUrl => "https://osu.ppy.sh/api/";

        protected override string Target => $"get_beatmaps";

        public GetMap(int id)
        {
            AddParameter("k", MainConfig.APIKey);
            AddParameter("b", id.ToString());
        }
    }
}
