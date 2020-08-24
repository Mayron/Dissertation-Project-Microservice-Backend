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
    public class CreateProject
    {
        public class Command : IRequest<ValidationResult>
        {
            public NewProjectInputModel Model { get; }

            public Command(NewProjectInputModel model)
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
                var sagaExecutionCommand = new ExecuteCreateProjectSagaCommand
                {
                    Name = command.Model.Name,
                    About = command.Model.About,
                    Tags = command.Model.Tags,
                    Visibility = command.Model.Visibility
                };

                var context = _builderFactory.CreateSagaContext<CreateProjectSaga>(sagaExecutionCommand)
                    .SetClientCallback(command.Model.ConnectionId, command.Model.Callback)
                    .Build();

                _actorSystemService.ExecuteSaga(context);
                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}