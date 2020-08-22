using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;

namespace OpenSpark.ApiGateway.Handlers.Queries
{
    public class FetchProjectDetails
    {
        public class Query : IRequest<Unit>
        {
            public string ProjectId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string connectionId, string callback, string projectId)
            {
                ConnectionId = connectionId;
                Callback = callback;
                ProjectId = projectId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilderFactory _builderFactory;

            public Handler(IActorSystem actorSystem, IMessageContextBuilderFactory builder)
            {
                _actorSystem = actorSystem;
                _builderFactory = builder;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var context = _builderFactory.CreateQueryContext(new ProjectDetailsQuery { ProjectId = query.ProjectId })
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Projects)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}