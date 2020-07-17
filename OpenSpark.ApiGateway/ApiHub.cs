using MediatR;
using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;
using System;

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
        /// Makes a request to the actor model to send the client with the specified connection ID new news feed posts.
        /// Posts are sent later asynchronously from a message hub event.
        /// </summary>
        public void FetchNewsFeed(string callback)
        {
            _mediator.Send(new FetchNewsFeed.Query(Context.ConnectionId, callback));
        }

        public void FetchGroup(string groupId, string callback)
        {
            _mediator.Send(new FetchBasicGroupDetails.Query(groupId, Context.ConnectionId, callback));
        }

        public void FetchGroupCategories(string callback)
        {
            _mediator.Send(new FetchGroupCategories.Query(Context.ConnectionId, callback));
        }

        public void Subscribe(string callback, string token)
        {
            if (Guid.TryParse(token, out var transactionId))
            {
                _mediator.Send(new SubscribeToTransaction.Query(Context.ConnectionId, transactionId, callback));
            }
        }
    }
}