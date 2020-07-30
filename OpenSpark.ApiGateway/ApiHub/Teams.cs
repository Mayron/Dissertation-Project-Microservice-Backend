// ReSharper disable UnusedMember.Global

using Microsoft.AspNetCore.Authorization;
using OpenSpark.ApiGateway.Handlers.Queries;

namespace OpenSpark.ApiGateway.ApiHub
{
    public partial class ApiHub
    {
        [Authorize]
        public void FetchTeams(string callback, string projectId) =>
            _mediator.Send(new FetchTeams.Query(Context.ConnectionId, callback, projectId));
    }
}