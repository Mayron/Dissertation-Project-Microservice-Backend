using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchProjectConnections
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string connectionId, string callback)
            {
                ConnectionId = connectionId;
                Callback = callback;
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
                    _actorSystemService.SendPayloadToClient(query.ConnectionId, query.Callback, _user.Projects);
                    return Unit.Task;
                }

                _actorSystemService.SendProjectsMessage(new UserProjectsQuery
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