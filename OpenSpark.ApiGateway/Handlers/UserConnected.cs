using MediatR;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Domain;
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
            private readonly IRemoteActorSystemService _remoteActorSystemService;
            private readonly IFirestoreService _firestoreService;

            public Handler(IRemoteActorSystemService remoteActorSystemService, IFirestoreService firestoreService)
            {
                _remoteActorSystemService = remoteActorSystemService;
                _firestoreService = firestoreService;
            }

            public async Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var authId = query.User.GetFirebaseAuth();
                User user = null;

                if (!string.IsNullOrWhiteSpace(authId))
                    user = await _firestoreService.GetUserAsync(authId, cancellationToken);

                var command = new ConnectUserCommand
                {
                    ConnectionId = query.ConnectionId,
                    User = user
                };

                _remoteActorSystemService.Send(command);

                return await Unit.Task;
            }
        }
    }
}