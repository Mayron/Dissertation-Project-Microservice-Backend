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
    public class FetchUserProjects
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public bool Subscriptions { get; }
            public bool Owned { get; }

            public Query(string connectionId, string callback, bool subscriptions = false, bool owned = false)
            {
                ConnectionId = connectionId;
                Callback = callback;
                Subscriptions = subscriptions;
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
                    _actorSystemService.SendErrorToClient(query.ConnectionId, query.Callback, "Unauthorized");
                    return Unit.Task;
                }

                if (_user.Projects.Count == 0)
                {
                    _actorSystemService.SendPayloadToClient(
                        query.ConnectionId, query.Callback, ImmutableList<string>.Empty);
                    return Unit.Task;
                }

                _actorSystemService.SendRemoteMessage(RemoteSystem.Projects, new UserProjectsQuery
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    User = _user,
                    OwnedProjects = query.Owned,
                    Subscriptions = query.Subscriptions
                });

                return Unit.Task;
            }
        }
    }
}