using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Discussions;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared.Commands.Teams;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class ChangePermission
    {
        public class Command : IRequest<ValidationResult>
        {
            public ChangePermissionInputModel Model { get; }

            public Command(ChangePermissionInputModel model)
            {
                Model = model;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;

            public Handler(IActorSystem actorSystem, IMessageContextBuilder builder)
            {
                _actorSystem = actorSystem;
                _builder = builder;
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                var remoteCommand = new ChangePermissionCommand
                {
                    TeamId = command.Model.TeamId,
                    Enabled = command.Model.Enabled,
                    Permission = command.Model.Permission
                };

                // Does not use a connectionId/client callback (fire and forget) 
                var context = _builder.CreateCommandContext(remoteCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .ForRemoteSystem(RemoteSystem.Teams)
                    .Build();

                _actorSystem.ExecuteCommand(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}