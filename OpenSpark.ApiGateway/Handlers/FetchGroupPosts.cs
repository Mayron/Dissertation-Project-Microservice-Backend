using MediatR;
using OpenSpark.ApiGateway.Actors.MultiQueryHandlers;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchGroupPosts
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
            private readonly IActorSystemService _actorSystemService;
            private readonly IMessageContextBuilderService _builder;

            public Handler(IActorSystemService actorSystemService, IMessageContextBuilderService builder)
            {
                _actorSystemService = actorSystemService;
                _builder = builder;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var remoteQuery = new GroupPostsQuery { GroupId = query.GroupId };

                var context = _builder.CreateMultiQueryContext<GetPostsMultiQueryHandler, GetPostsAggregator>()
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .AddQuery(remoteQuery, RemoteSystem.Discussions)
                    .Build();

                _actorSystemService.SendMultiQuery(context);

                return Unit.Task;
            }
        }
    }
}