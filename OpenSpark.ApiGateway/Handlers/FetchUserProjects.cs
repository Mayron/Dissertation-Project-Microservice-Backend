using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;

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
                    _actorSystemService.CallbackHandler.Tell(new PayloadEvent
                    {
                        ConnectionId = query.ConnectionId,
                        Callback = query.Callback,
                        Error = "Unauthorized"
                    });

                    return Unit.Task;
                }

                if (_user.Projects.Count == 0)
                {
                    _actorSystemService.CallbackHandler.Tell(new PayloadEvent
                    {
                        ConnectionId = query.ConnectionId,
                        Callback = query.Callback,
                        Payload = _user.Projects
                    });

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