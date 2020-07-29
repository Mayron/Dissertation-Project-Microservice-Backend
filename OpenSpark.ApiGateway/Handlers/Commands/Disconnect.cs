using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Services;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class Disconnect
    {
        public class Command : IRequest
        {
            public string ConnectionId { get; }

            public Command(string connectionId)
            {
                ConnectionId = connectionId;
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly IActorSystem _actorSystem;

            public Handler(IActorSystem actorSystem)
            {
                _actorSystem = actorSystem;
            }

            public Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                _actorSystem.PublishDisconnection(command.ConnectionId);
                return Unit.Task;
            }
        }
    }
}