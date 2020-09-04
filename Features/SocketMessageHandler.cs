using System;
using System.Collections.Generic;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.Extensions.Logging;

namespace CloudHeavenApi.Features
{
    public class SocketMessageHandler
    {
        private readonly ILogger<IWebSocketService> _logger;

        private readonly Dictionary<MessageType, Func<object, string, ResponseData>> handlerMap =
            new Dictionary<MessageType, Func<object, string, ResponseData>>();

        public SocketMessageHandler(ILogger<IWebSocketService> logger)
        {
            _logger = logger;
        }

        public void RegisterHandler(MessageType type, Func<object, string, ResponseData> action)
        {
            handlerMap[type] = (obj, id) =>
            {
                try
                {
                    return action.Invoke(obj, id);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while handling socket ({id}) with type {type}, message: {e.Message}");
                    return new ResponseData
                    {
                        Type = ResponseType.Error,
                        Data = e.Message,
                        Receiver = Receiver.Self
                    };
                }
            };
        }

        public bool TryGetHandler(MessageType type, out Func<object, string, ResponseData> action)
        {
            return handlerMap.TryGetValue(type, out action);
        }
    }


    public enum MessageType
    {
        BrowserMessage,
        MinecraftMessage,
        ServerOnline
    }

    public enum ResponseType
    {
        Error,
        ServerInfo,
        Message
    }

    public enum Receiver
    {
        Minecraft,
        Browser,
        All,
        Self
    }
}