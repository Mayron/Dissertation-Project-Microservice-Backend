using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway
{
    public class ApiHub : Hub
    {
        private readonly IMediator _mediator;

        public ApiHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// This is used to manage user actor life times by creating the user
        /// actor or removing the expiry timestamp for that connected user.
        /// </summary>
        public override Task OnConnectedAsync()
        {
            var query = new UserConnected.Query(
                Context.User,
                Context.ConnectionId);

            _mediator.Send(query);
            return base.OnConnectedAsync();
        }
        
        /// <summary>
        /// This is used to manage user actor life times by setting a expiry
        /// timestamp for that disconnected user.
        /// </summary>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _mediator.Send(new UserDisconnected.Query(Context.ConnectionId));
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Makes a request to the actor model to send the client with the specified connection ID new news feed posts.
        /// Posts are sent later asynchronously from a message hub event.
        /// </summary>
        public void FetchNewsFeed()
        {
            _mediator.Send(new FetchNewsFeed.Query(Context.ConnectionId));
        }

        public void Subscribe(string token, string callback)
        {
            if (Guid.TryParse(token, out var transactionId))
            {
                _mediator.Send(new SubscribeToTransaction.Query(Context.ConnectionId, transactionId, callback));
            }
        }
    }
}
