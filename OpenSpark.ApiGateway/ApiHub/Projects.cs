// ReSharper disable UnusedMember.Global

using Microsoft.AspNetCore.Authorization;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHub
{
    public partial class ApiHub
    {
        public void FetchProject(string callback, string projectId) =>
            _mediator.Send(new FetchProjectDetails.Query(projectId, Context.ConnectionId, callback));

        [Authorize]
        public void FetchUserProjects(string callback) =>
            _mediator.Send(new FetchUserProjects.Query(Context.ConnectionId, callback, owned: true));

        [Authorize]
        public void FetchUserSubscriptions(string callback) =>
            _mediator.Send(new FetchUserProjects.Query(Context.ConnectionId, callback, subscriptions: true));

        [Authorize]
        public void FetchProjectConnections(string callback) =>
            _mediator.Send(new FetchUserProjects.Query(Context.ConnectionId, callback, subscriptions: true));
    }
}