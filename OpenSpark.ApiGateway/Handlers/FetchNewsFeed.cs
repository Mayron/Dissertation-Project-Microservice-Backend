using System.Security.Claims;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchNewsFeed
    {
        public class Query : IRequest
        {
            public string ConnectionId { get; }
            public ClaimsPrincipal AuthUser { get; }

            public Query(string connectionId, ClaimsPrincipal authUser)
            {
                ConnectionId = connectionId;
                AuthUser = authUser;
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
                var user = await _firestoreService.GetUserAsync(query.AuthUser, cancellationToken);

                _actorSystemService.SendDiscussionsMessage(new NewsFeedQuery
                {
                    ConnectionId = query.ConnectionId,
                    User = user
                });

                return Unit.Value;
            }
        }
    }
}