using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
{
    public class FetchCategories
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string connectionId, string callback)
            {
                ConnectionId = connectionId;
                Callback = callback;
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
                var context = _builderFactory.CreateQueryContext(new CategoriesQuery())
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Groups)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}