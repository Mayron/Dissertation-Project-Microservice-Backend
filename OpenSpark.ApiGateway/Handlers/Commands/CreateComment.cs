using MediatR;
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
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;

            public Handler(IActorSystem actorSystem, IMessageContextBuilder builder)
            {
                _actorSystem = actorSystem;
                _builder = builder;
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                var remoteCommand = new CreateCommentCommand
                {
                    Body = command.Model.Body,
                    PostId = command.Model.PostId,
                };

                var context = _builder.CreateCommandContext(remoteCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .ForRemoteSystem(RemoteSystem.Discussions)
                    .Build();

                _actorSystem.ExecuteCommand(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}