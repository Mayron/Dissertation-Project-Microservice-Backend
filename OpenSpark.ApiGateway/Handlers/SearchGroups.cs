using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.Shared;

namespace OpenSpark.ApiGateway.Handlers
{
    public class SearchGroups
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public string SearchQuery { get; }

            public Query(string connectionId, string callback, string searchQuery)
            {
                ConnectionId = connectionId;
                Callback = callback;
                SearchQuery = searchQuery;
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
                _actorSystemService.SendRemoteMessage(RemoteSystem.Groups, new SearchGroupsQuery
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    SearchQuery = query.SearchQuery,
                    User = _user,
                });

                return Unit.Task;
            }
        }
    }
}