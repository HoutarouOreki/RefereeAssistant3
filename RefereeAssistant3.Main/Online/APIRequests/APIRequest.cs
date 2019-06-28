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

        public new async Task<T> RunTask()
        {
            PrepareRequest();
            var res = await Client.ExecuteGetTaskAsync(Request);
            if (res.IsSuccessful)
            {
                try
                { return JsonConvert.DeserializeObject<T>(res.Content); }
                catch { return default; }
            }
            else
                return default;
        }

        public delegate void ResponseSuccesfulGenericEventHandler(T obj, HttpStatusCode code);
        new public event ResponseSuccesfulGenericEventHandler Success;
    }
}
