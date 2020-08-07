﻿using MediatR;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
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
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;

            public Handler(IActorSystem actorSystem, IMessageContextBuilder builder)
            {
                _actorSystem = actorSystem;
                _builder = builder;
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

                var context = _builder.CreateCommandContext(remoteCommand)
                    .ForRemoteSystem(RemoteSystem.Teams)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .Build();

                _actorSystem.ExecuteCommand(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}