using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Http;

namespace CloudHeavenApi.MiddleWaresAndFilters
{
    public class WebSocketMiddleware : IMiddleware
    {
        private readonly IWebSocketService _socketService;

        public WebSocketMiddleware(IWebSocketService socketService)
        {
            _socketService = socketService;
        }


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await next(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _socketService.OnConnected(socket);

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await _socketService.ReceiveAsync(socket, result, buffer);
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _socketService.OnDisconnected(socket);
                }

            });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                    cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
