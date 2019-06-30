using RestSharp;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public class APIResponse<T>
    {
        public IRestResponse Response;
        public T Object;

        public APIResponse(IRestResponse res, T obj)
        {
            Response = res;
            Object = obj;
        }
    }
}
