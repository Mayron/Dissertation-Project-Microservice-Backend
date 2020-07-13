using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Services.SDK;
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
            private readonly IRemoteActorSystemService _remoteActorSystemService;

            public Handler(IRemoteActorSystemService remoteActorSystemService)
            {
                _remoteActorSystemService = remoteActorSystemService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var command = new FetchNewsFeedCommand
                {
                    ConnectionId = query.ConnectionId
                };

                _remoteActorSystemService.Send(command);
                return Unit.Task;
            }
        }
    }
}