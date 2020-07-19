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
    public class FetchGroupDetails
    {
        public class Query : IRequest<Unit>
        {
            public string GroupId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string groupId, string connectionId, string callback)
            {
                GroupId = groupId;
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
                _actorSystemService.SendGroupsMessage(new GroupDetailsQuery
                {
                    GroupId = query.GroupId,
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    User = _user
                });

                return Unit.Task;
            }
        }
    }
}