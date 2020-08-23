using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace CloudHeavenApi.Implementation
{
    public class WebSocketTable : IEnumerable<KeyValuePair<string, WebSocket>>
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets =
            new ConcurrentDictionary<string, WebSocket>();

        public WebSocket this[string id] => _sockets.TryGetValue(id, out var socket) ? socket : null;

        public string this[WebSocket socket] => _sockets.FirstOrDefault(p => p.Value == socket).Key;


        public IEnumerator<KeyValuePair<string, WebSocket>> GetEnumerator()
        {
            return _sockets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task RemoveSocket(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closed by the Restful Api Server",
                    CancellationToken.None);
        }

        public async Task Add(WebSocket socket, string clientToken)
        {
            var id = string.IsNullOrEmpty(clientToken) ? Guid.NewGuid().ToString() : clientToken;
            if (_sockets.ContainsKey(id))
            {
                await RemoveSocket(id);
            }
            _sockets[id] = socket;
        }
    }
}