using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchNewsFeed
    {
        public class Query : IRequest
        {
            public string ConnectionId { get; }

            public Query(string connectionId)
            {
                ConnectionId = connectionId;
            }
        }

        public class Handler : IRequestHandler<Query>
        {
            private readonly IActorSystemService _actorSystemService;

            public Handler(IActorSystemService actorSystemService)
            {
                _actorSystemService = actorSystemService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var command = new FetchNewsFeedCommand
                {
                    ConnectionId = query.ConnectionId
                };

                _actorSystemService.SendDiscussionsCommand(command);
                return Unit.Task;
            }
        }
    }
}