using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Shared.Payloads;

namespace OpenSpark.ApiGateway.Services
{
    // This class handles the EVENTS being emitted from ApiHubBridgeActor
    public class EventEmitterService : IEventEmitterService
    {
        private readonly IHubContext<ApiHub> _hubContext;

        public EventEmitterService(IHubContext<ApiHub> liveChatHubContext)
        {
            _hubContext = liveChatHubContext;
        }

        public void ReceivedPayload(NewsFeedPostsPayload payload)
        {
            _hubContext.Clients.Client(payload.ConnectionId).SendAsync("NewsFeedUpdate", payload.Posts);
        }
    }
}
