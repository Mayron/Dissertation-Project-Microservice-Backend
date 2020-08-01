// ReSharper disable UnusedMember.Global
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;
using OpenSpark.ApiGateway.Handlers.Commands;
using OpenSpark.ApiGateway.Handlers.Queries;

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
        public void FetchNewsFeed(string callback, List<string> seen) =>
            _mediator.Send(new FetchNewsFeed.Query(Context.ConnectionId, callback, seen));

        public void FetchComments(string callback, string postId) =>
            _mediator.Send(new FetchComments.Query(Context.ConnectionId, callback, postId));

        public override Task OnConnectedAsync()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                // Need to set them to online
                _mediator.Send(new Connected.Command(Context.ConnectionId));
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // Notify all subscription actors that user with ConnectionId has disconnected
            _mediator.Send(new Disconnect.Command(Context.ConnectionId));

            return base.OnDisconnectedAsync(exception);
        }
    }
}