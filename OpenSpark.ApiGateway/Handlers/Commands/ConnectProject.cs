using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.Domain;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class ConnectProject
    {
        public class Command : IRequest<ValidationResult>
        {
            public ConnectProjectInputModel Model { get; }

            public Command(ConnectProjectInputModel model)
            {
                Model = model;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilderFactory _builderFactory;
            private readonly User _user;

            public Handler(IActorSystem actorSystem, IHttpContextAccessor context, IMessageContextBuilderFactory builder)
            {
                _actorSystem = actorSystem;
                _builderFactory = builder;
                _user = context.GetFirebaseUser();
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                // TODO: Move logic to controller filter
                if (!_user.Projects.Contains(command.Model.ProjectId))
                    return Task.FromResult(new ValidationResult(false, "Failed to verify project"));

                if (!_user.Groups.Contains(command.Model.GroupId))
                    return Task.FromResult(new ValidationResult(false,
                        "You do not have permission to connect to the selected group."));

                var sagaExecutionCommand = new ExecuteConnectProjectSagaCommand
                {
                    GroupId = command.Model.GroupId,
                    ProjectId = command.Model.ProjectId
                };

                var context = _builderFactory.CreateSagaContext<ConnectProjectSaga>(sagaExecutionCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .Build();

                _actorSystem.ExecuteSaga(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}