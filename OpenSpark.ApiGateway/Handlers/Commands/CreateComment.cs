﻿using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Discussions;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class CreateComment
    {
        public class Command : IRequest<ValidationResult>
        {
            public CommentInputModel Model { get; }

            public Command(CommentInputModel model)
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
                var remoteCommand = new CreateCommentCommand
                {
                    Body = command.Model.Body,
                    PostId = command.Model.PostId,
                };

                var context = _builderFactory.CreateCommandContext(remoteCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .ForRemoteSystem(RemoteSystem.Discussions)
                    .Build();

                _actorSystemService.ExecuteCommand(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}