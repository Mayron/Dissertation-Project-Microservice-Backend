using Akka.Actor;
using MediatR;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class CreateGroup
    {
        public class Command : IRequest<ValidationResult>
        {
            public NewGroupInputModel Model { get; }
            public ClaimsPrincipal User { get; }

            public Command(NewGroupInputModel model, ClaimsPrincipal user)
            {
                Model = model;
                User = user;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystemService actorSystemService, IFirestoreService firestoreService)
            {
                _actorSystemService = actorSystemService;
                _firestoreService = firestoreService;
            }

            public async Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                // Verify
                var user = await _firestoreService.GetUserAsync(command.User, cancellationToken);

                if (user == null)
                    return new ValidationResult(false, "Failed to validate user request");

                var transactionId = Guid.NewGuid();
                _actorSystemService.SagaManager.Tell(new ExecuteCreateGroupSagaCommand
                {
                    SagaName = nameof(CreateGroupSagaActor),
                    TransactionId = transactionId,
                    Name = command.Model.Name,
                    About = command.Model.About,
                    CategoryId = command.Model.CategoryId,
                    Tags = command.Model.Tags,
                    Connecting = command.Model.Connected,
                    User = user
                });

                return new ValidationResult(true, transactionId.ToString());
            }
        }
    }
}