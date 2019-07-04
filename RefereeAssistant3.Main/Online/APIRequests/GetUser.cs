using RefereeAssistant3.Main.APIModels;
using RestSharp;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public class GetUsers : APIRequest<APIUser[]>
    {
        public override Method Method => Method.GET;

        public override string Title => "Download user";

        protected override string BaseUrl => "https://osu.ppy.sh/api/";

        protected override string Target => $"get_user";

        public GetUsers(int? userId, string username)
        {
            AddParameter("k", MainConfig.APIKey);
            AddParameter("u", userId.HasValue ? userId.Value.ToString() : username);
        }
    }
}
