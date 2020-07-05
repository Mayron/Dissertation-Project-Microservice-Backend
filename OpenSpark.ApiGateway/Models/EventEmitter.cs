using Microsoft.AspNetCore.SignalR;
using OpenSpark.ActorModel.Services;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Models
{
    // This class handles the EVENTS being emitted from SignalRBridgeActor
    public class EventEmitter : IEventEmitter
    {
        private readonly IHubContext<ApiHub> _liveChatHubContext;

        public EventEmitter(IHubContext<ApiHub> liveChatHubContext)
        {
            _liveChatHubContext = liveChatHubContext;
        }

        public void ChatMessageSent(User user, ChatMessage message)
        {
            _liveChatHubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public void UserConnected(User user)
        {
   
        }
    }
}
