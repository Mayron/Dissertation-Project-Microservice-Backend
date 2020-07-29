using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Actors.MultiQueryHandlers;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
{
    public class FetchGroupPosts
    {
        public class Query : IRequest<Unit>
        {
            public string GroupId { get; }
            public List<string> Seen { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string connectionId, string callback, string groupId, List<string> seen)
            {
                ConnectionId = connectionId;
                Callback = callback;
                GroupId = groupId;
                Seen = seen;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;

            public Handler(IActorSystem actorSystem, IMessageContextBuilder builder)
            {
                _actorSystem = actorSystem;
                _builder = builder;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var remoteQuery = new GroupPostsQuery
                {
                    GroupId = query.GroupId,
                    Seen = query.Seen
                };

                var context = _builder.CreateMultiQueryContext<GetPostsMultiQueryHandler, GetPostsAggregator>()
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .AddQuery(remoteQuery, RemoteSystem.Discussions)
                    .Build();

                _actorSystem.SendMultiQuery(context);

                return Unit.Task;
            }
        }
    }
}