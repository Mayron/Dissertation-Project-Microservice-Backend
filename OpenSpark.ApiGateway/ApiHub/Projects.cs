// ReSharper disable UnusedMember.Global
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHub
{
    public partial class ApiHub
    {
        public void FetchUserProjects(string callback) =>
            _mediator.Send(new FetchUserProjects.Query(Context.ConnectionId, callback, owned: true));

        public void FetchUserSubscriptions(string callback) =>
            _mediator.Send(new FetchUserProjects.Query(Context.ConnectionId, callback, subscriptions: true));
    }
}