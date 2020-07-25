using MediatR;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchGroupProjects
    {
        public class Query : IRequest<Unit>
        {
            public string GroupId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }
            public int Amount { get; }

            public Query(string connectionId, string callback, string groupId, int amount)
            {
                ConnectionId = connectionId;
                Callback = callback;
                GroupId = groupId;
                Amount = amount;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IMessageContextBuilderService _builder;

            public Handler(IActorSystemService actorSystemService, IMessageContextBuilderService builder)
            {
                _actorSystemService = actorSystemService;
                _builder = builder;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var remoteQuery = new GroupProjectsQuery
                {
                    GroupId = query.GroupId,
                    TakeAmount = query.Amount
                };

                var context = _builder.CreateQueryContext(remoteQuery)
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Projects)
                    .Build();

                _actorSystemService.SendRemoteQuery(context);

                return Unit.Task;
            }
        }
    }
}