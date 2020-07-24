using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
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
            private readonly IMessageContextBuilderService _builder;
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context, IMessageContextBuilderService builder)
            {
                _actorSystemService = actorSystemService;
                _builder = builder;
                _user = context.GetFirebaseUser();
            }

            public async Task<ValidationResult> Handle(Command command, CancellationToken cancellationToken)
            {
                if (_user == null)
                    return new ValidationResult(false, "Failed to validate user request");

                var sagaExecutionCommand = new ExecuteCreateProjectSagaCommand
                {
                    Name = command.Model.Name,
                    About = command.Model.About,
                    Tags = command.Model.Tags,
                    Visibility = command.Model.Visibility
                };

                var context = _builder.CreateSagaContext<CreateProjectSaga>(sagaExecutionCommand).Build();
                await _actorSystemService.RegisterAndExecuteSagaAsync(context);

                return new ValidationResult(true, context.TransactionId.ToString());
            }
        }
    }
}