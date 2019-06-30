using RestSharp;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public class PostNewMatch : APIRequest<APIMatch>
    {
        public override Method Method => Method.POST;

        public override string Title => "Create new match";

        protected override string BaseUrl => MainConfig.ServerURL;

        protected override string Target => "/api/matches/new";

        public PostNewMatch(APIMatch match) => SetJsonObject(match);
    }
}
