using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchProjectDetails
    {
        public class Query : IRequest<Unit>
        {
            public string ProjectId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string projectId, string connectionId, string callback)
            {
                ProjectId = projectId;
                ConnectionId = connectionId;
                Callback = callback;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context)
            {
                _actorSystemService = actorSystemService;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                _actorSystemService.SendProjectsMessage(new ProjectDetailsQuery
                {
                    ProjectId = query.ProjectId,
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    User = _user
                });

                return Unit.Task;
            }
        }
    }
}