using Microsoft.AspNetCore.SignalR;

namespace OpenSpark.ApiGateway.Services
{
    public interface IEventEmitterService
    {
        void BroadcastToClient(string connectionId, string clientSideMethod, object eventData);

        void BroadcastToGroup(string groupId, string clientSideMethod, object eventData);
    }

    public class EventEmitterService : IEventEmitterService
    {
        private readonly IHubContext<ApiHub.ApiHub> _hubContext;

        public EventEmitterService(IHubContext<ApiHub.ApiHub> liveChatHubContext)
        {
            _hubContext = liveChatHubContext;
        }

        public void BroadcastToClient(string connectionId, string clientSideMethod, object eventData)
        {
            _hubContext.Clients.Client(connectionId).SendAsync(clientSideMethod, eventData);
        }

        public void BroadcastToGroup(string groupId, string clientSideMethod, object eventData)
        {
            _hubContext.Clients.Group(groupId).SendAsync(clientSideMethod, eventData);
        }
    }
}