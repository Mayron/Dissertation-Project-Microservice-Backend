using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class UserDisconnected
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

            public async Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var command = new DisconnectUserCommand
                {
                    ConnectionId = query.ConnectionId
                };

                // TODO: Should I tell other contexts I have connected?
                _actorSystemService.SendDiscussionsCommand(command);

                return await Unit.Task;
            }
        }
    }
}
