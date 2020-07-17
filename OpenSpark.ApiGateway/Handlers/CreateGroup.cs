using Akka.Actor;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Middleware;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.ApiGateway.Handlers
{
    public class CreateGroup
    {
        public class Command : IRequest<ValidationResult>
        {
            public NewGroupInputModel Model { get; }

            public Command(NewGroupInputModel model)
            {
                Model = model;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context)
            {
                _actorSystemService = actorSystemService;
                _user = context.GetFirebaseUser();
            }

            public async Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                // Verify
                if (_user == null)
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
                    User = _user
                });

                return new ValidationResult(true, transactionId.ToString());
            }
        }
    }
}