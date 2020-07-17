using Microsoft.AspNetCore.SignalR;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHubEndpoints
{
    public partial class ApiHub : Hub
    {
        public void FetchGroup(string groupId, string callback)
        {
            _mediator.Send(new FetchBasicGroupDetails.Query(groupId, Context.ConnectionId, callback));
        }

        public void FetchUserGroups(string callback)
        {
            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback));
        }

        public void FetchGroupCategories(string callback)
        {
            _mediator.Send(new FetchGroupCategories.Query(Context.ConnectionId, callback));
        }
    }
}