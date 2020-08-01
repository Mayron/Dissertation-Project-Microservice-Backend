using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries.Teams;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.ApiGateway.Handlers.Queries.Teams
{
    public class FetchTeamPermissions
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
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystem actorSystem, IMessageContextBuilder builder, IFirestoreService firestoreService)
            {
                _actorSystem = actorSystem;
                _builder = builder;
                _firestoreService = firestoreService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var context = _builder.CreateQueryContext(new TeamMembersQuery { TeamId = query.TeamId })
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .OnPayloadReceived(OnPayloadReceived)
                    .ForRemoteSystem(RemoteSystem.Teams)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }

            public void OnPayloadReceived(object payload)
            {
                if (!(payload is List<TeamMemberViewModel> members)) return;

                foreach (var member in members)
                {
                    var user = _firestoreService.GetUserAsync(member.UserId).Result;

                    member.Name = user.DisplayName;
                    member.LastOnline = user.IsOnline ? "Online" : user.LastOnline.ToTimeAgoFormat();
                }
            }
        }
    }
}