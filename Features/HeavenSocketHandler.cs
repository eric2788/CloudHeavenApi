using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using CloudHeavenApi.Implementation;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudHeavenApi.Features
{
    public class HeavenSocketHandler : WebSocketHandler
    {
        private readonly ICacheService<Identity> _cacheService;

        public HeavenSocketHandler(WebSocketTable socketTable, ILogger<IWebSocketService> logger,
            ICacheService<Identity> cacheService) : base(socketTable, logger)
        {
            _cacheService = cacheService;
        }

        public override async Task OnSocketConnected(WebSocket socket)
        {
            await Task.Run(() =>
            {
                var id = WebSocketTable[socket];
                Logger.LogInformation($"Socket({id}) has been connected.");
            });
        }

        public override async Task OnSocketClosed(WebSocket socket, string id)
        {
            await Task.Run(() =>
            {
                var id = WebSocketTable[socket];
                Logger.LogInformation($"Socket({id}) has been disconnected.");
            });
        }

        public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var container = GetAsObject<SocketMessageContainer>(result, buffer);
            var id = WebSocketTable[socket];
            Logger.LogInformation(
                $"Received message from socket {id}, message: {JsonConvert.SerializeObject(container)}");
            ResponseData output = null;
            var ignoreMc = false;

            Logger.LogInformation($"type: {container.Type}, Data: {container.Data}");
            Logger.LogInformation($"Data With JSON: {JsonConvert.SerializeObject(container.Data)}");

            // Socket Sent From MC Server

            var containerData = container.Data;

            if (id.Equals("mc-server-socket"))
            {
                ignoreMc = true;
                if (containerData.CanDeserialize<McMessageData>() && container.Type == RequestType.Message)
                {
                    var data = containerData.JsonDeserialize<McMessageData>();
                    output = new ResponseData
                    {
                        Type = ResponseType.Message,
                        Data = new OutputContainer
                        {
                            FromMC = true,
                            Identity = new Identity
                            {
                                NickName = "",
                                UserName = data.UserName,
                                UUID = data.Guid
                            },
                            Message = data.Message,
                            Server = data.Server
                        }
                    };
                }
                else if (containerData.CanDeserialize<McServerData>() && container.Type == RequestType.SendBrowser)
                {
                    var serverData = containerData.JsonDeserialize<McServerData>();
                    var online = serverData.Online;
                    Logger.LogInformation($"Received Server Information: {JsonConvert.SerializeObject(online)}");
                    output = new ResponseData
                    {
                        Type = ResponseType.ServerInfo,
                        Data = online
                    };
                }
                else
                {
                    Logger.LogWarning(
                        $"The message received from {id} is not match the pattern MCData with type {container.Type}");
                }
            }

            // Socket Sent From Browser

            else
            {
                if (containerData.CanDeserialize<BrowserMessageData>() && container.Type == RequestType.Message)
                {
                    var msgData = containerData.JsonDeserialize<BrowserMessageData>();
                    if (!_cacheService.TryGetItem(msgData.ClientToken, out var identity))
                    {
                        Logger.LogWarning(
                            $"The message received from {id} does not have any identity in cache (Not Login?)");
                        return;
                    }

                    output = new ResponseData
                    {
                        Type = ResponseType.Message,
                        Data = new OutputContainer
                        {
                            FromMC = false,
                            Identity = identity,
                            Message = msgData.Message,
                            Server = "Website"
                        }
                    };
                }
                else
                {
                    Logger.LogWarning(
                        $"The message received from {id} is not match the pattern BrowserData with type {container.Type}");
                }
            }

            if (output is null) return;

            await BroadcastAsync(output, ignoreMc);
        }
    }

    public class SocketMessageContainer
    {
        public RequestType Type { get; set; }
        public object Data { get; set; }
    }

    public class BrowserMessageData
    {
        public string ClientToken { get; set; }
        public string Message { get; set; }
    }

    public class McMessageData
    {
        public Guid Guid { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string Server { get; set; }
    }

    public class ResponseData
    {
        public ResponseType Type { get; set; }
        public object Data { get; set; }
    }

    public class McServerData
    {
        public IEnumerable<string> Online { get; set; }
    }

    public class OutputContainer
    {
        public Identity Identity { get; set; }
        public string Message { get; set; }
        public bool FromMC { get; set; }
        public string Server { get; set; }
    }

    public enum ResponseType
    {
        ServerInfo,
        Message
    }

    public enum RequestType
    {
        Message,
        SendBrowser
    }

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
    }
}