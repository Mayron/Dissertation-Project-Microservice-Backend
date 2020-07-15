using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Sagas;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Sagas.ExecutionCommands;
using Raven.Client;

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
                    TransactionId = transactionId,
                    Group = new Group
                    {
                        Name = query.Model.Name,
                        About = query.Model.About,
                        CategoryId = query.Model.CategoryId,
                        Tags = query.Model.Tags,
                        Connected = query.Model.Connected,
                        OwnerUserId = user.UserId,
                        Members = new List<Member> { new Member {  UserId = user.UserId } }
                    },
                    User = user
                });

                return new ValidationResult(true, transactionId.ToString());
            }
        }
    }
}