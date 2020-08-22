using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
{
    public class FetchGroupDetails
    {
        public class Query : IRequest<Unit>
        {
            public string GroupId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string connectionId, string callback, string groupId)
            {
                ConnectionId = connectionId;
                Callback = callback;
                GroupId = groupId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilderFactory _builderFactory;

            public Handler(IActorSystem actorSystem, IMessageContextBuilderFactory builder)
            {
                _actorSystem = actorSystem;
                _builderFactory = builder;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var context = _builderFactory.CreateQueryContext(new GroupDetailsQuery { GroupId = query.GroupId })
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Groups)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}