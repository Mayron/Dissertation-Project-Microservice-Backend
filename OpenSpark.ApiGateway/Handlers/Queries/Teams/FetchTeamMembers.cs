using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries.Teams;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers.Queries.Teams
{
    public class FetchTeamMembers
    {
        public class Query : IRequest<Unit>
        {
            public string ConnectionId { get; }
            public string Callback { get; }
            public string TeamId { get; }

            public Query(string connectionId, string callback, string teamId)
            {
                ConnectionId = connectionId;
                Callback = callback;
                TeamId = teamId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;

            public Handler(IActorSystem actorSystem, IMessageContextBuilder builder)
            {
                _actorSystem = actorSystem;
                _builder = builder;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var context = _builder.CreateQueryContext(new TeamPermissionsQuery { TeamId = query.TeamId })
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Teams)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}