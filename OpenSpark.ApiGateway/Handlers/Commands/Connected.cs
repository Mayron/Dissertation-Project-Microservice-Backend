using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class Connected
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
            private readonly IFirestoreService _firestoreService;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public Handler(IFirestoreService firestoreService, IHttpContextAccessor httpContextAccessor)
            {
                _firestoreService = firestoreService;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var user = _httpContextAccessor.HttpContext.GetFirebaseUser();

                user.IsOnline = true;
                user.LastOnline = DateTime.Now;

                await _firestoreService.UpdateUserField(user, "isOnline", user.IsOnline, cancellationToken);
                await _firestoreService.UpdateUserField(user, "lastOnline", user.LastOnline, cancellationToken);

                return Unit.Value;
            }
        }
    }
}