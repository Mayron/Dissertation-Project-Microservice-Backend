using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.Shared;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchUserGroups
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public bool Memberships { get; }
            public bool Owned { get; }

            public Query(string connectionId, string callback, bool memberships = false, bool owned = false)
            {
                ConnectionId = connectionId;
                Callback = callback;
                Memberships = memberships;
                Owned = owned;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context)
            {
                _actorSystemService = actorSystemService;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user == null)
                {
                    _actorSystemService.SendErrorToClient(
                        query.ConnectionId, query.Callback, "Unauthorized");

                    return Unit.Task;
                }

                if (_user.Groups.Count == 0)
                {
                    _actorSystemService.SendPayloadToClient(
                        query.ConnectionId, query.Callback, ImmutableList<string>.Empty);

                    return Unit.Task;
                }

                _actorSystemService.SendRemoteMessage(RemoteSystem.Groups,
                    new UserGroupsQuery
                    {
                        ConnectionId = query.ConnectionId,
                        Callback = query.Callback,
                        User = _user,
                        OwnedGroups = query.Owned,
                        Memberships = query.Memberships
                    });

                return Unit.Task;
            }
        }
    }
}