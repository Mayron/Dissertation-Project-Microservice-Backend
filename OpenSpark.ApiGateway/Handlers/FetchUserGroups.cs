using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;

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
            private readonly IMessageContextBuilderService _builder;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context, IMessageContextBuilderService builder)
            {
                _actorSystemService = actorSystemService;
                _builder = builder;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user == null)
                {
                    _actorSystemService.SendErrorToClient(query.Callback, query.ConnectionId, "Unauthorized");
                    return Unit.Task;
                }

                if (_user.Groups.Count == 0)
                {
                    _actorSystemService.SendEmptyPayloadToClient(query.Callback, query.ConnectionId);
                    return Unit.Task;
                }

                var remoteQuery = new UserGroupsQuery
                {
                    OwnedGroups = query.Owned,
                    Memberships = query.Memberships
                };

                var context = _builder.CreateQueryContext(remoteQuery)
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Groups)
                    .Build();

                _actorSystemService.SendRemoteQuery(context);

                return Unit.Task;
            }
        }
    }
}