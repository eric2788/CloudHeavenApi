using CloudHeavenApi.Services;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CloudHeavenApi.Implementation
{
    public abstract class WebSocketHandler : IWebSocketService
    {
        protected WebSocketTable WebSocketTable { get; set; }
        protected ILogger<IWebSocketService> Logger { get; set; }

        protected WebSocketHandler(WebSocketTable socketTable, ILogger<IWebSocketService> logger)
        {
            WebSocketTable = socketTable;
            Logger = logger;
        }


        public async Task OnConnected(WebSocket socket)
        {
            WebSocketTable.Add(socket);
            await OnSocketConnected(socket);
        }

        public abstract Task OnSocketConnected(WebSocket socket);

        public async Task OnDisconnected(WebSocket socket)
        {
            await WebSocketTable.RemoveSocket(WebSocketTable[socket]);
            await OnSocketConnected(socket);
        }

        public abstract Task OnSocketClosed(WebSocket socket);

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);

        protected string GetAsString(WebSocketReceiveResult result, byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        protected T GetAsObject<T>(WebSocketReceiveResult result, byte[] buffer)
        {
            var data = GetAsString(result, buffer);
            return JsonConvert.DeserializeObject<T>(data);
        }

        protected async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                    offset: 0,
                    count: message.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }

        protected async Task SendMessageAsync(WebSocket socket, object data)
        {
            if (socket.State != WebSocketState.Open)
                return;
            var message = JsonConvert.SerializeObject(data);
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                    offset: 0,
                    count: message.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string sid, string message)
        {
            await SendMessageAsync(WebSocketTable[sid], message);
        }

        public async Task BroadcastAsync(string message, bool ignoreMc = false)
        {
            foreach (var (id, socket) in WebSocketTable)
            {
                if (id.Equals("mc-server-socket") && ignoreMc) continue;
                if (socket.State == WebSocketState.Open)
                    await SendMessageAsync(socket, message);
            }
        }

        public async Task BroadcastAsync(object data, bool ignoreMc = false)
        {
            foreach (var (id, socket) in WebSocketTable)
            {
                if (id.Equals("mc-server-socket") && ignoreMc) continue;
                if (socket.State == WebSocketState.Open)
                    await SendMessageAsync(socket, data);
            }
        }

        public async Task SendMessageAsync(string sid, object data)
        {
            await SendMessageAsync(WebSocketTable[sid], data);
        }
    }

    
}
