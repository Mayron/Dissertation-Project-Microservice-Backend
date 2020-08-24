using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Teams;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class CreateTeam
    {
        public class Command : IRequest<ValidationResult>
        {
            public NewTeamInputModel Model { get; }

            public Command(NewTeamInputModel model)
            {
                Model = model;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IMessageContextBuilderFactory _builderFactory;

            public Handler(IActorSystemService actorSystem, IMessageContextBuilderFactory builder)
            {
                _actorSystemService = actorSystem;
                _builderFactory = builder;
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                var remoteCommand = new CreateTeamCommand
                {
                    Color = command.Model.Color,
                    Description = command.Model.Description,
                    ProjectId = command.Model.ProjectId,
                    TeamName = command.Model.Name
                };

                var context = _builderFactory.CreateCommandContext(remoteCommand)
                    .ForRemoteSystem(RemoteSystem.Teams)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .Build();

                _actorSystemService.ExecuteCommand(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}