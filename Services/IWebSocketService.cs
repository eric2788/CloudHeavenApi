using System.Net.WebSockets;
using System.Threading.Tasks;

namespace CloudHeavenApi.Services
{
    public interface IWebSocketService
    {
        Task OnConnected(WebSocket socket, string clientToken);
        Task OnDisconnected(WebSocket socket);
        Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
        Task SendMessageAsync(string sid, object data);
        Task BroadcastAsync(object data, bool ignoreMc = false);
        Task BroadcastAsync(string message, bool ignoreMc = false);
        Task SendMessageAsync(string sid, string message);
    }
}