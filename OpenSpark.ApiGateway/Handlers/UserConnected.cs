using MediatR;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class UserConnected
    {
        public class Query : IRequest
        {
            public ClaimsPrincipal User { get; }
            public string ConnectionId { get; }

            public Query(ClaimsPrincipal user, string connectionId)
            {
                User = user;
                ConnectionId = connectionId;
            }
        }

        public class Handler : IRequestHandler<Query>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystemService actorSystemService, IFirestoreService firestoreService)
            {
                _actorSystemService = actorSystemService;
                _firestoreService = firestoreService;
            }

            public async Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var user = await _firestoreService.GetUserAsync(query.User, cancellationToken);

                var command = new ConnectUserCommand
                {
                    ConnectionId = query.ConnectionId,
                    User = user
                };

                // TODO: Should I tell other services I have connected?
                _actorSystemService.SendDiscussionsCommand(command);

                return await Unit.Task;
            }
        }
    }
}