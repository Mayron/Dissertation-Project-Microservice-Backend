using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHubEndpoints
{
    public partial class ApiHub : Hub
    {
        public void FetchGroup(string callback, string groupId) =>
            _mediator.Send(new FetchBasicGroupDetails.Query(groupId, Context.ConnectionId, callback));

        public void FetchUserGroups(string callback) =>
            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback, owned: true));

        public void FetchUserMemberships(string callback) =>
            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback, memberships: true));

        public void FetchCategories(string callback) =>
            _mediator.Send(new FetchCategories.Query(Context.ConnectionId, callback));
    }
}