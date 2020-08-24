using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries.Teams;
using OpenSpark.Shared.ViewModels;

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
            private readonly IActorSystemService _actorSystemService;
            private readonly IMessageContextBuilderFactory _builderFactory;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystemService actorSystem, IMessageContextBuilderFactory builder, IFirestoreService firestoreService)
            {
                _actorSystemService = actorSystem;
                _builderFactory = builder;
                _firestoreService = firestoreService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var context = _builderFactory.CreateQueryContext(new TeamMembersQuery { TeamId = query.TeamId })
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .OnPayloadReceived(OnPayloadReceived)
                    .ForRemoteSystem(RemoteSystem.Teams)
                    .Build();

                _actorSystemService.SendQuery(context);

                return Unit.Task;
            }

            public void OnPayloadReceived(PayloadEvent @event)
            {
                if (!(@event.Payload is List<TeamMemberViewModel> members)) return;

                foreach (var member in members)
                {
                    var user = _firestoreService.GetUserAsync(member.UserId).Result;

                    member.Name = user.DisplayName;
                    member.LastOnline = user.IsOnline ? "Online" : user.LastOnline.ToTimeAgoFormat();
                    member.UserId = null;
                }
            }
        }
    }
}