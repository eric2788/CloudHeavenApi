using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CloudHeavenApi.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CloudHeavenApi.Implementation
{
    public abstract class WebSocketHandler : IWebSocketService
    {
        protected WebSocketHandler(WebSocketTable socketTable, ILogger<IWebSocketService> logger)
        {
            WebSocketTable = socketTable;
            Logger = logger;
        }

        protected WebSocketTable WebSocketTable { get; set; }
        protected ILogger<IWebSocketService> Logger { get; set; }


        public async Task OnConnected(WebSocket socket, string clientToken)
        {
            await WebSocketTable.Add(socket, clientToken);
            await OnSocketConnected(socket);
        }

        public async Task OnDisconnected(WebSocket socket)
        {
            var id = WebSocketTable[socket];
            await WebSocketTable.RemoveSocket(id);
            await OnSocketClosed(socket, id);
        }

        public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);

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
            var message = JsonConvert.SerializeObject(data);
            await BroadcastAsync(message, ignoreMc);
        }

        public async Task SendMessageAsync(string sid, object data)
        {
            await SendMessageAsync(WebSocketTable[sid], data);
        }

        public abstract Task OnSocketConnected(WebSocket socket);

        public abstract Task OnSocketClosed(WebSocket socket, string id);

        protected string GetAsString(WebSocketReceiveResult result, byte[] buffer)
        {
            var url = Encoding.UTF8.GetString(buffer, 0, result.Count);
            return HttpUtility.UrlDecode(url, Encoding.UTF8);
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

            var msg = HttpUtility.UrlEncode(message);

            if (msg is null)
            {
                return;
            }

            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg),
                    0,
                    msg.Length),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        protected async Task SendMessageAsync(WebSocket socket, object data)
        {
            var message = JsonConvert.SerializeObject(data);
            await SendMessageAsync(socket, message);
        }
    }
}