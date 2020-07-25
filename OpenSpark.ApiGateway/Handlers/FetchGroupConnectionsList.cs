using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Actors.MultiQueryHandlers;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchGroupConnectionsList
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public string ProjectId { get; }

            public Query(string connectionId, string callback, string projectId)
            {
                ConnectionId = connectionId;
                Callback = callback;
                ProjectId = projectId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IMessageContextBuilderService _builder;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context, IMessageContextBuilderService builder)
            {
                _actorSystemService = actorSystemService;
                _builder = builder;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user == null || _user.Projects.Count == 0)
                {
                    _actorSystemService.SendErrorToClient(query.Callback, query.ConnectionId, "Unauthorized");
                    return Unit.Task;
                }

                if (_user.Groups.Count == 0)
                {
                    _actorSystemService.SendEmptyPayloadToClient(query.Callback, query.ConnectionId);
                    return Unit.Task;
                }

                var context = _builder.CreateMultiQueryContext<MultiQueryParallelHandler, GroupConnectionsListAggregator>()
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .AddQuery(new UserGroupsQuery { OwnedGroups = true }, RemoteSystem.Groups)
                    .AddQuery(new ProjectDetailsQuery { ProjectId = query.ProjectId }, RemoteSystem.Projects)
                    .Build();

                _actorSystemService.SendMultiQuery(context);

                return Unit.Task;
            }
        }
    }
}