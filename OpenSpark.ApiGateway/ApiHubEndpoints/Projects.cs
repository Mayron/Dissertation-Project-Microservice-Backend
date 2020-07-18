using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHubEndpoints
{
    public partial class ApiHub : Hub
    {
        public void FetchUserProjects(string callback) =>
            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback, owned: true));


        public void FetchUserSubscriptions(string callback)
        {
            //            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback, "memberships"));
        }
    }
}