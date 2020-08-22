using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.MultiQueryHandlers;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Domain;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
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
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilderFactory _builderFactory;
            private readonly User _user;

            public Handler(IActorSystem actorSystem, IHttpContextAccessor context, IMessageContextBuilderFactory builder)
            {
                _actorSystem = actorSystem;
                _builderFactory = builder;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user.Projects.Count == 0)
                {
                    var metaData = new MetaData { ConnectionId = query.ConnectionId, Callback = query.Callback };
                    _actorSystem.SendErrorToClient(metaData, "Unauthorized");
                    return Unit.Task;
                }

                if (_user.Groups.Count == 0)
                {
                    var metaData = new MetaData { ConnectionId = query.ConnectionId, Callback = query.Callback };
                    _actorSystem.SendEmptyPayloadToClient(metaData);
                    return Unit.Task;
                }

                var context = _builderFactory
                    .CreateMultiQueryContext<MultiQueryParallelHandler, GroupConnectionsListAggregator>()
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .AddQuery(new UserGroupsQuery { OwnedGroups = true }, RemoteSystem.Groups)
                    .AddQuery(new ProjectDetailsQuery { ProjectId = query.ProjectId }, RemoteSystem.Projects)
                    .Build();

                _actorSystem.SendMultiQuery(context);

                return Unit.Task;
            }
        }
    }
}