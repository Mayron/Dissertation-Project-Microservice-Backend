using MediatR;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Builders;

namespace OpenSpark.ApiGateway.Handlers.Commands
{
    public class CreatePost
    {
        public class Command : IRequest<ValidationResult>
        {
            public NewPostInputModel Model { get; }

            public Command(NewPostInputModel model)
            {
                Model = model;
            }
        }

        public class Handler : IRequestHandler<Command, ValidationResult>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilderFactory _builderFactory;

            public Handler(IActorSystem actorSystem, IMessageContextBuilderFactory builder)
            {
                _actorSystem = actorSystem;
                _builderFactory = builder;
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                var sagaExecutionCommand = new ExecuteCreatePostSagaCommand
                {
                    GroupId = command.Model.GroupId,
                    Title = command.Model.Title,
                    Body = command.Model.Body,
                };

                var context = _builderFactory.CreateSagaContext<CreatePostSaga>(sagaExecutionCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .Build();

                _actorSystem.ExecuteSaga(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}