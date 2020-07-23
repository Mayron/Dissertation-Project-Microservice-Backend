using Akka.Actor;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.SagaExecutionCommands;
using OpenSpark.Shared.ViewModels;
using System;
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
            private readonly User _user;

            public Handler(IActorSystemService actorSystemService, IHttpContextAccessor context)
            {
                _actorSystemService = actorSystemService;
                _user = context.GetFirebaseUser();
            }

            public Task<ValidationResult> Handle(Query query, CancellationToken cancellationToken)
            {
                if (_user == null)
                    return Task.FromResult(new ValidationResult(false, "Failed to validate user request"));

                var transactionId = Guid.NewGuid();
                _actorSystemService.ExecuteSaga(new ExecuteAddPostSagaCommand
                {
                    SagaName = nameof(CreatePostSagaActor),
                    TransactionId = transactionId,
                    GroupId = query.GroupId,
                    Title = query.Title,
                    Body = query.Body,
                    User = _user
                });

                return Task.FromResult(new ValidationResult(true, transactionId.ToString()));
            }
        }
    }
}