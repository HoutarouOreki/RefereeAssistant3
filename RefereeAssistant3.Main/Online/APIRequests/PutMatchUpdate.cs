using RefereeAssistant3.Main.Online.APIModels;
using RestSharp;
using System;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public class PutMatchUpdate : APIRequest<APIMatch>
    {
        private readonly int matchId;

        public override Method Method => Method.PUT;

        public override string Title => "Update match";

        protected override string BaseUrl => MainConfig.ServerURL;

        protected override string Target => $"api/matches/update/{matchId}";

        public PutMatchUpdate(APIMatch match)
        {
            if (match.Id < 0)
                throw new ArgumentOutOfRangeException();
            matchId = match.Id;
            SetJsonObject(match);
        }
    }
}
