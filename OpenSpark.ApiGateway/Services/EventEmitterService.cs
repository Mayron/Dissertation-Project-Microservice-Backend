using Microsoft.AspNetCore.SignalR;
using OpenSpark.Shared.Events;

namespace OpenSpark.ApiGateway.Services
{
    public interface IEventEmitterService
    {
        void ReceivedPayload(NewsFeedReceivedEvent payload);
        void ReceivedEvent(PostAddedEvent ev);
    }

    // This class handles the EVENTS being emitted from ApiHubBridgeActor
    public class EventEmitterService : IEventEmitterService
    {
        private readonly IHubContext<ApiHub> _hubContext;

        public EventEmitterService(IHubContext<ApiHub> liveChatHubContext)
        {
            _hubContext = liveChatHubContext;
        }

        // TODO: Can this be renamed to Event?
        public void ReceivedPayload(NewsFeedReceivedEvent payload)
        {
            _hubContext.Clients.Client(payload.ConnectionId).SendAsync("NewsFeedUpdate", payload.Posts);
        }

        public void ReceivedEvent(PostAddedEvent ev)
        {
            var groupId = ev.GroupId;
            _hubContext.Clients.Group(groupId).SendAsync("PostAdded", ev.Post);
        }
    }
}
