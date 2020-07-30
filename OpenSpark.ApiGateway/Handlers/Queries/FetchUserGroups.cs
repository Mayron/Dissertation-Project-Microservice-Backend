using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Domain;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
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
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;
            private readonly User _user;

            public Handler(IActorSystem actorSystem, IHttpContextAccessor context, IMessageContextBuilder builder)
            {
                _actorSystem = actorSystem;
                _builder = builder;
                _user = context.GetFirebaseUser();
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user.Groups.Count == 0)
                {
                    var metaData = new MetaData { ConnectionId = query.ConnectionId, Callback = query.Callback };
                    _actorSystem.SendEmptyPayloadToClient(metaData);
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

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}