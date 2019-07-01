using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public abstract class APIRequest<T> : APIRequestBase
    {
        protected override void OnRequestComplete(IRestResponse arg1, RestRequestAsyncHandle arg2)
        {
            base.OnRequestComplete(arg1, arg2);
            Schedule(() =>
            {
                if (arg1.IsSuccessful)
                    Success?.Invoke(JsonConvert.DeserializeObject<T>(Response.Content), Response.StatusCode);
            });
        }

        public new async Task<APIResponse<T>> RunTask()
        {
            if (Client == null)
                return new APIResponse<T>(null, default);

            PrepareRequest();
            var res = await Client?.ExecuteTaskAsync(Request);

            if (res?.IsSuccessful == true)
                return new APIResponse<T>(res, JsonConvert.DeserializeObject<T>(res?.Content));
            else
                return new APIResponse<T>(res, default);
        }

        public delegate void ResponseSuccesfulGenericEventHandler(T obj, HttpStatusCode code);
        public new event ResponseSuccesfulGenericEventHandler Success;
    }
}
