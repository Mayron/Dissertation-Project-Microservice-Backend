using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
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
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystem actorSystem, IHttpContextAccessor httpContextAccessor, IFirestoreService firestoreService)
            {
                _actorSystem = actorSystem;
                _httpContextAccessor = httpContextAccessor;
                _firestoreService = firestoreService;
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var identity = _httpContextAccessor.HttpContext.User?.Identity;
                if (identity != null && identity.IsAuthenticated)
                {
                    var user = _httpContextAccessor.HttpContext.GetFirebaseUser();

                    user.IsOnline = false;
                    user.LastOnline = DateTime.Now;

                    await _firestoreService.UpdateUserField(user, "isOnline", user.IsOnline, cancellationToken);
                    await _firestoreService.UpdateUserField(user, "lastOnline", user.LastOnline, cancellationToken);
                }

                _actorSystem.PublishDisconnection(command.ConnectionId);
                return Unit.Value;
            }
        }
    }
}