using System.Collections.Generic;
using MediatR;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Actors.MultiQueryHandlers;
using OpenSpark.ApiGateway.Actors.PayloadAggregators;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchNewsFeed
    {
        public class Query : IRequest
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public List<string> Seen { get; }

            public Query(string connectionId, string callback, List<string> seen)
            {
                ConnectionId = connectionId;
                Callback = callback;
                Seen = seen;
            }
        }

        public class Handler : IRequestHandler<Query>
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
                var context = _builder.CreateMultiQueryContext<GetPostsMultiQueryHandler, GetPostsAggregator>()
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .AddQuery(new NewsFeedQuery { Seen = query.Seen }, RemoteSystem.Discussions)
                    .Build();

                _actorSystemService.SendMultiQuery(context);

                return Unit.Task;
            }
        }
    }
}