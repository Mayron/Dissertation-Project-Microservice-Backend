using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Discussions;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Builders;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class ChangeVote
    {
        public class Command : IRequest<ValidationResult>
        {
            public ChangeVoteInputModel Model { get; }

            public Command(ChangeVoteInputModel model)
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
                var remoteCommand = new ChangeVoteCommand
                {
                    CommentId = command.Model.CommentId,
                    PostId = command.Model.PostId,
                    Amount = command.Model.Vote
                };

                // Does not use a connectionId/client callback (fire and forget) 
                var context = _builderFactory.CreateCommandContext(remoteCommand)
                    .ForRemoteSystem(RemoteSystem.Discussions)
                    .Build();

                _actorSystemService.ExecuteCommand(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}