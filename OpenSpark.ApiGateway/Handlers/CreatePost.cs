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
    public class CreatePost
    {
        public class Query : IRequest<ValidationResult>
        {
            public string Body { get; set; }
            public string Title { get; set; }
            public string GroupId { get; set; }

            public Query(NewPostInputModel model)
            {
                GroupId = model.GroupId;
                Body = model.Body;
                Title = model.Title;
            }
        }

        public class Handler : IRequestHandler<Query, ValidationResult>
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

            public async Task<ValidationResult> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user == null)
                    return new ValidationResult(false, "Failed to validate user request");

                var sagaExecutionCommand = new ExecuteCreatePostSagaCommand
                {
                    GroupId = query.GroupId,
                    Title = query.Title,
                    Body = query.Body,
                };

                var context = _builder.CreateSagaContext<CreatePostSaga>(sagaExecutionCommand).Build();
                await _actorSystemService.RegisterAndExecuteSagaAsync(context);

                return new ValidationResult(true, context.TransactionId.ToString());
            }
        }
    }
}