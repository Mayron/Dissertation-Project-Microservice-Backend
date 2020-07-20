using Akka.Actor;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class ConnectProject
    {
        public class Command : IRequest<ValidationResult>
        {
            public string GroupId { get; set; }
            public string ProjectId { get; set; }

            public Command(ConnectProjectInputModel model)
            {
                ProjectId = model.ProjectId;
                GroupId = model.GroupId;
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

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                if (_user == null || !_user.Projects.Contains(command.ProjectId))
                    return Task.FromResult(new ValidationResult(false, "Failed to validate user request"));

                if (!_user.Groups.Contains(command.GroupId))
                    return Task.FromResult(new ValidationResult(false, 
                        "You do not have permission to connect to the selected group."));

                var transactionId = Guid.NewGuid();
                _actorSystemService.ExecuteSaga(new ExecuteConnectProjectSagaCommand
                {
                    SagaName = nameof(CreateProjectSagaActor),
                    TransactionId = transactionId,
                    User = _user,
                    GroupId = command.GroupId,
                    ProjectId = command.ProjectId
                });

                return Task.FromResult(new ValidationResult(true, transactionId.ToString()));
            }
        }
    }
}