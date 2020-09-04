using System.Collections.Generic;
using System.Linq;
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
        private readonly SocketMessageHandler _messageHandler;

        public HeavenSocketHandler(WebSocketTable socketTable, ILogger<IWebSocketService> logger,
            ICacheService<Identity> cacheService, SocketMessageHandler messageHandler) : base(socketTable, logger)
        {
            _cacheService = cacheService;
            _messageHandler = messageHandler;

            _messageHandler.RegisterHandler(MessageType.BrowserMessage, (o, id) =>
            {
                var data = o.JsonDeserialize<SocketMessageContainer.BrowserMessageData>();
                if (!_cacheService.TryGetItem(data.ClientToken, out var identity))
                {
                    Logger.LogWarning(
                        $"The message received from {id} does not have any identity in cache (Not Login?)");
                    return new ResponseData
                    {
                        Type = ResponseType.Error,
                        Data = "Invalid Token, Unknown User Identity.",
                        Receiver = Receiver.Self
                    };
                }

                var format = string.IsNullOrEmpty(identity.NickName)
                    ? $"[網站] {identity.UserName}: {data.Message}"
                    : $"[網站] {identity.UserName}({identity.NickName}) {data.Message}";

                return new ResponseData
                {
                    Type = ResponseType.Message,
                    Data = format,
                    Receiver = Receiver.All
                };
            });

            _messageHandler.RegisterHandler(MessageType.MinecraftMessage, (obj, id) =>
            {
                if (!id.Equals("mc-server-socket"))
                    return new ResponseData
                    {
                        Type = ResponseType.Error,
                        Data = "socket id is not belongs to mc server",
                        Receiver = Receiver.Self
                    };

                var data = obj.JsonDeserialize<SocketMessageContainer.McMessageData>();

                if (!data.Token.Equals("965d1e06f5df5708ae154e7751f9c631"))
                    return new ResponseData
                    {
                        Type = ResponseType.Error,
                        Data = "mc server token mismatched",
                        Receiver = Receiver.Self
                    };

                return new ResponseData
                {
                    Type = ResponseType.Message,
                    Data = data.Message,
                    Receiver = Receiver.Browser
                };
            });

            _messageHandler.RegisterHandler(MessageType.ServerOnline, (obj, id) =>
            {
                if (!id.Equals("mc-server-socket"))
                    return new ResponseData
                    {
                        Type = ResponseType.Error,
                        Data = "socket id is not belongs to mc server",
                        Receiver = Receiver.Self
                    };

                var data = obj.JsonDeserialize<IEnumerable<string>>();

                logger.LogInformation($"Received Server Info Online Count {data.Count()}");
                return new ResponseData
                {
                    Type = ResponseType.ServerInfo,
                    Data = data,
                    Receiver = Receiver.Browser
                };
            });
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

            Logger.LogInformation($"type: {container.Type}, Data: {container.Data}");
            Logger.LogInformation($"Data With JSON: {JsonConvert.SerializeObject(container.Data)}");


            if (!_messageHandler.TryGetHandler(container.Type, out var handle))
            {
                Logger.LogWarning($"Unknown Socket Message Type from socket {id}, Skipped");
                return;
            }

            var output = handle(container.Data, id);

            if (output is null) return;

            switch (output.Receiver)
            {
                case Receiver.All:
                    await BroadcastAsync(output);
                    break;
                case Receiver.Browser:
                    await BroadcastAsync(output, true);
                    break;
                case Receiver.Minecraft:
                    await SendMessageAsync("mc-server-socket", output);
                    break;
                case Receiver.Self:
                    await SendMessageAsync(socket, output);
                    break;
                default:
                    return;
            }
        }
    }
}