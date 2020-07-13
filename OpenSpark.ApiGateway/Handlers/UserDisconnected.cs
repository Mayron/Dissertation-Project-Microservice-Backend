using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Services.SDK;
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
            private readonly IRemoteActorSystemService _remoteActorSystemService;

            public Handler(IRemoteActorSystemService remoteActorSystemService)
            {
                _remoteActorSystemService = remoteActorSystemService;
            }

            public async Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var command = new DisconnectUserCommand
                {
                    ConnectionId = query.ConnectionId
                };

                _remoteActorSystemService.Send(command);

                return await Unit.Task;
            }
        }
    }
}
