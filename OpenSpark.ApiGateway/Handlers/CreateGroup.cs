using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Sagas;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
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

                var sagaExecutionCommand = new ExecuteCreateGroupSagaCommand
                {
                    Name = command.Model.Name,
                    About = command.Model.About,
                    Visibility = command.Model.Visibility,
                    CategoryId = command.Model.CategoryId,
                    Tags = command.Model.Tags,
                    Connecting = command.Model.Connected,
                };

                var context = _builder.CreateSagaContext<CreateGroupSaga>(sagaExecutionCommand).Build();
                await _actorSystemService.RegisterAndExecuteSagaAsync(context);

                return new ValidationResult(true, context.TransactionId.ToString());
            }
        }
    }
}