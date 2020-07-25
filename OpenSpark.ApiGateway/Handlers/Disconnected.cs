using MediatR;
using OpenSpark.ApiGateway.Services;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class Disconnected
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
                _actorSystemService.PublishDisconnection(query.ConnectionId);
                return Unit.Task;
            }
        }
    }
}