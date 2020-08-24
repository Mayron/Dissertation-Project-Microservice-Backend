using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Domain;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
{
    public class FetchUserProjects
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public bool Subscriptions { get; }
            public bool Owned { get; }

            public Query(string connectionId, string callback, bool subscriptions = false, bool owned = false)
            {
                ConnectionId = connectionId;
                Callback = callback;
                Subscriptions = subscriptions;
                Owned = owned;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IMessageContextBuilderFactory _builderFactory;
            private readonly User _user;

            public Handler(IActorSystemService actorSystem, IHttpContextAccessor context, IMessageContextBuilderFactory builder)
            {
                _actorSystemService = actorSystem;
                _builderFactory = builder;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user.Projects.Count == 0)
                {
                    var metaData = new MetaData { ConnectionId = query.ConnectionId, Callback = query.Callback };
                    _actorSystemService.SendEmptyPayloadToClient(metaData);
                    return Unit.Task;
                }

                var remoteQuery = new UserProjectsQuery
                {
                    OwnedProjects = query.Owned,
                    Subscriptions = query.Subscriptions
                };

                var context = _builderFactory.CreateQueryContext(remoteQuery)
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Projects)
                    .Build();

                _actorSystemService.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}