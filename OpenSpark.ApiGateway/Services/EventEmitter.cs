using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using OpenSpark.Shared.Events.Payloads;

namespace OpenSpark.ApiGateway.Services
{
    public interface IEventEmitter
    {
        void BroadcastToClient(PayloadEvent @event);
//        void BroadcastToGroup(string groupId, string clientSideMethod, object eventData);
        void RegisterCallback(Guid metaDataQueryId, Action<PayloadEvent> contextOnPayloadReceived);
    }

    public class EventEmitter : IEventEmitter
    {
        private readonly IHubContext<ApiHub.ApiHub> _hubContext;

        // TODO: Remove if unused after X seconds
        private readonly Dictionary<Guid, Action<PayloadEvent>> _callbacks;

        public EventEmitter(IHubContext<ApiHub.ApiHub> liveChatHubContext)
        {
            _hubContext = liveChatHubContext;
            _callbacks = new Dictionary<Guid, Action<PayloadEvent>>();
        }

        public void BroadcastToClient(PayloadEvent @event)
        {
            var duration = DateTime.Now - @event.MetaData.CreatedAt;
            Console.WriteLine($"{@event.MetaData.Callback} finished in {duration.Milliseconds}ms");

                var queryId = @event.MetaData.Id;
            if (_callbacks.ContainsKey(queryId))
            {
                _callbacks[queryId](@event);
                _callbacks.Remove(queryId);
            }

            var (connectionId, clientSideMethod, eventData) = @event;
            _hubContext.Clients.Client(connectionId).SendAsync(clientSideMethod, eventData);
        }

//        public void BroadcastToGroup(string groupId, string clientSideMethod, object eventData)
//        {
//            _hubContext.Clients.Group(groupId).SendAsync(clientSideMethod, eventData);
//        }

        public void RegisterCallback(Guid queryId, Action<PayloadEvent> callback)
        {
            _callbacks.Add(queryId, callback);
        }
    }
}