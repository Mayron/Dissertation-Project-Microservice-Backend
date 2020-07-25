// ReSharper disable UnusedMember.Global
using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHub
{
    public partial class ApiHub : Hub
    {
        private readonly IMediator _mediator;

        public ApiHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Makes a request to the actor model to send the client with the specified connection ID new news feed posts.
        /// Posts are sent later asynchronously from a message hub event.
        /// </summary>
        public void FetchNewsFeed(string callback) =>
            _mediator.Send(new FetchNewsFeed.Query(Context.ConnectionId, callback));

        [Authorize]
        public void Subscribe(string callback, string token)
        {
            if (Guid.TryParse(token, out var transactionId))
            {
                _mediator.Send(new SubscribeToTransaction.Query(Context.ConnectionId, transactionId, callback));
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // Notify all subscription actors that user with ConnectionId has disconnected
            _mediator.Send(new Disconnected.Query(Context.ConnectionId));

            return base.OnDisconnectedAsync(exception);
        }
    }
}