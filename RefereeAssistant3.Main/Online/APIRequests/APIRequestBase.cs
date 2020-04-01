using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main.Online.APIRequests
{
    public abstract class APIRequestBase : Component
    {
        protected readonly RestClient Client;

        protected abstract string BaseUrl { get; }
        protected abstract string Target { get; }

        private string uri => $"{Client.BaseUrl.OriginalString}/{Target}";

        public abstract Method Method { get; }
        public abstract string Title { get; }
        protected IRestResponse Response;

        public DateTime StartTime;
        public readonly Bindable<RequestState> State = new Bindable<RequestState>(RequestState.Pending);

        protected RestRequest Request = new RestRequest();
        private RestRequestAsyncHandle asyncHandle;

        protected bool IncludeSecret { get; set; }

        public APIRequestBase()
        {
            if (Uri.IsWellFormedUriString(BaseUrl, UriKind.Absolute))
                Client = new RestClient($"{BaseUrl}");
        }

        public void Run()
        {
            if (Client == null)
            {
                Fail?.Invoke("Could not create a request", HttpStatusCode.BadRequest);
                return;
            }
            PrepareRequest();
            asyncHandle = Client?.ExecuteAsync(Request, OnRequestComplete);
        }

        public virtual Task<IRestResponse> RunTask()
        {
            if (Client == null)
            {
                Fail?.Invoke("Could not create a request", HttpStatusCode.BadRequest);
                return null;
            }
            PrepareRequest();
            return Client?.ExecuteAsync(Request);
        }

        protected void SetJsonObject(object obj) => Request.AddJsonBody(JsonConvert.SerializeObject(obj));

        protected void AddParameter(string name, string value) => Request.AddParameter(name, value);

        protected void PrepareRequest()
        {
            AlwaysPresent = true;

            StartTime = DateTime.UtcNow;

            Request.Resource = Target;
            Request.Method = Method;
            Request.Timeout = 10000;
            Logger.Log($"Creating request to {uri} ({Method})", LoggingTarget.Network);
        }

        public void Abort()
        {
            if (asyncHandle == null)
                throw new Exception("Tried to abort a not started request");
            asyncHandle.Abort();
            State.Value = RequestState.Aborted;
        }

        protected virtual void OnRequestComplete(IRestResponse arg1, RestRequestAsyncHandle arg2)
        {
            Logger.Log($"Request to {uri} ({Method}) complete", LoggingTarget.Network);
            Schedule(() =>
            {
                Response = arg1;

                if (Response.IsSuccessful)
                {
                    Success?.Invoke(Response.Content, Response.StatusCode);
                    State.Value = RequestState.Completed;
                }
                else
                {
                    var ResponseMessage = string.Empty;
                    if (Response.StatusCode == HttpStatusCode.ServiceUnavailable || string.IsNullOrEmpty(Response.Content))
                    {
                        ResponseMessage = "Servers seem to be down right now";
                    }
                    else
                    {
                        ResponseMessage = Response.Content;
                    }
                    State.Value = RequestState.Failed;
                    Fail?.Invoke(ResponseMessage, Response.StatusCode);
                }
            });
        }

        public delegate void ResponseSuccesfulEventHandler(object obj, HttpStatusCode code);
        public delegate void ResponseFailedEventHandler(object obj, HttpStatusCode code);
        public event ResponseSuccesfulEventHandler Success;
        public event ResponseFailedEventHandler Fail;
    }

    public enum RequestState
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Aborted = 3,
    }
}
