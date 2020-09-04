using CloudHeavenApi.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CloudHeavenApi.Models
{
    public static class SocketMessageExtension
    {
        public static bool CanDeserialize<T>(this object o)
        {
            try
            {
                JsonConvert.DeserializeObject<T>(o.ToString());
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static T JsonDeserialize<T>(this object o)
        {
            return JsonConvert.DeserializeObject<T>(o.ToString());
        }

        public static IServiceCollection LoadSocketMessageHandler(this IServiceCollection service)
        {
            service.AddSingleton<SocketMessageHandler>();
            return service;
        }
    }

    public class SocketMessageContainer
    {
        public MessageType Type { get; set; }
        public object Data { get; set; }

        public class BrowserMessageData
        {
            public string ClientToken { get; set; }
            public string Message { get; set; }
        }

        public class McMessageData
        {
            public string Token { get; set; }
            public string Message { get; set; }
        }
    }


    public class ResponseData
    {
        public ResponseType Type { get; set; }
        public object Data { get; set; }
        public Receiver Receiver { get; set; }
    }
}