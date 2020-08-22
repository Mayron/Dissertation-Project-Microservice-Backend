using MediatR;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers.Commands
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
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilderFactory _builderFactory;

            public Handler(IActorSystem actorSystem, IMessageContextBuilderFactory builder)
            {
                _actorSystem = actorSystem;
                _builderFactory = builder;
            }

            public Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                var sagaExecutionCommand = new ExecuteCreateGroupSagaCommand
                {
                    Name = command.Model.Name,
                    About = command.Model.About,
                    Visibility = command.Model.Visibility,
                    CategoryId = command.Model.CategoryId,
                    Tags = command.Model.Tags,
                    Connecting = command.Model.Connected,
                };

                var context = _builderFactory.CreateSagaContext<CreateGroupSaga>(sagaExecutionCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .Build();

                _actorSystem.ExecuteSaga(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}