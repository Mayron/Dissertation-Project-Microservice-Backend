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

            public Query(string projectId, string connectionId, string callback)
            {
                ProjectId = projectId;
                ConnectionId = connectionId;
                Callback = callback;
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
                var context = _builder.CreateQueryContext(new ProjectDetailsQuery { ProjectId = query.ProjectId })
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .ForRemoteSystem(RemoteSystem.Projects)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }
        }
    }
}