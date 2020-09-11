using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CloudHeavenApi.MiddleWaresAndFilters
{
    public class WebSocketMiddleware : IMiddleware
    {
        private readonly ILogger<Startup> _logger;
        private readonly IWebSocketService _socketService;

        public WebSocketMiddleware(IWebSocketService socketService, ILogger<Startup> logger)
        {
            _socketService = socketService;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            _logger.LogInformation("WebSocketMiddleWare Invoking...");
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await next(context);
                return;
            }

            try
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var clientToken = context.Request.Query["client"].ToString();

                await _socketService.OnConnected(socket, clientToken);

                await Receive(socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                        await _socketService.ReceiveAsync(socket, result, buffer);

                    else if (result.MessageType == WebSocketMessageType.Close)
                        await _socketService.OnDisconnected(socket);
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}