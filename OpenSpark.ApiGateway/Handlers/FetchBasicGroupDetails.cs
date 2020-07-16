using Akka.Actor;
using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Actors.Sagas;

namespace OpenSpark.ApiGateway.Handlers
{
    public class FetchBasicGroupDetails
    {
        public class Query : IRequest<Unit>
        {
            public string GroupId { get; }
            public ClaimsPrincipal User { get; }
            public string ConnectionId { get; }

            public Query(string groupId, ClaimsPrincipal user, string connectionId)
            {
                GroupId = groupId;
                User = user;
                ConnectionId = connectionId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystemService actorSystemService, IFirestoreService firestoreService)
            {
                _actorSystemService = actorSystemService;
                _firestoreService = firestoreService;
            }

            public async Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                // Verify
                var user = await _firestoreService.GetUserAsync(query.User, cancellationToken);

                _actorSystemService.SendDiscussionsMessage(new BasicGroupDetailsQuery
                {
                    ConnectionId = query.ConnectionId
                });

                return Unit.Value;
            }
        }
    }
}