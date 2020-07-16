using Akka.Actor;
using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Sagas.ExecutionCommands;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Actors.Sagas;

namespace OpenSpark.ApiGateway.Handlers
{
    public class CreateGroup
    {
        public class Query : IRequest<ValidationResult>
        {
            public NewGroupInputModel Model { get; }
            public ClaimsPrincipal User { get; }

            public Query(NewGroupInputModel model, ClaimsPrincipal user)
            {
                Model = model;
                User = user;
            }
        }

        public class Handler : IRequestHandler<Query, ValidationResult>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystemService actorSystemService, IFirestoreService firestoreService)
            {
                _actorSystemService = actorSystemService;
                _firestoreService = firestoreService;
            }

            public async Task<ValidationResult> Handle(Query query, CancellationToken cancellationToken)
            {
                // Verify
                var user = await _firestoreService.GetUserAsync(query.User, cancellationToken);

                if (user == null)
                    return new ValidationResult(false, "Failed to validate user request");

                var transactionId = Guid.NewGuid();
                _actorSystemService.SagaManager.Tell(new ExecuteCreateGroupSagaCommand
                {
                    SagaName = nameof(CreateGroupSagaActor),
                    TransactionId = transactionId,
                    Name = query.Model.Name,
                    About = query.Model.About,
                    CategoryId = query.Model.CategoryId,
                    Tags = query.Model.Tags,
                    Connecting = query.Model.Connected,
                    OwnerUserId = user.AuthUserId,
                    User = user
                });

                return new ValidationResult(true, transactionId.ToString());
            }
        }
    }
}