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

        public override async Task OnSocketClosed(WebSocket socket)
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
            Logger.LogInformation($"Received message from socket {id}");
            ResponseData output = null;
            var ignoreMc = false;

            // Socket Sent From MC Server

            if (id.Equals("mc-server-socket"))
            {
                ignoreMc = true;
                switch (container.Data)
                {
                    case McMessageData data when container.Type == RequestType.Message:
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
                        break;
                    case McServerData serverData when container.Type == RequestType.SendBrowser:
                        var map = serverData.StatData;
                        Logger.LogInformation($"Received Server Information: {JsonConvert.SerializeObject(map)}");
                        output = new ResponseData
                        {
                            Type = ResponseType.ServerInfo,
                            Data = serverData
                        };
                        break;
                    default:
                        Logger.LogWarning(
                            $"The message received from {id} is not match the pattern {nameof(McMessageData)} with type {container.Type}");
                        break;
                }
            }

            // Socket Sent From Browser

            else
            {
                switch (container.Data)
                {
                    case BrowserMessageData msgData when container.Type == RequestType.Message:
                    {
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
                        break;
                    }
                    case BrowserCommandData cmdData when container.Type == RequestType.SendServer:
                        output = new ResponseData
                        {
                            Type = ResponseType.Command,
                            Data = cmdData
                        };
                        await SendMessageAsync("mc-server-socket", output);
                        return;
                    default:
                        Logger.LogWarning(
                            $"The message received from {id} is not match the pattern {nameof(McMessageData)}");
                        break;
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
        public Dictionary<string, object> StatData { get; set; }
    }

    public class BrowserCommandData
    {
        public string Command { get; set; }
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
        Message,
        Command
    }

    public enum RequestType
    {
        Message,
        SendServer,
        SendBrowser
    }
}