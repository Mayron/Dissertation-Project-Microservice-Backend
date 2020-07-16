using Akka.Actor;
using MediatR;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.Actors.Sagas;
using OpenSpark.Shared.Commands.SagaExecutionCommands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class CreatePost
    {
        public class Query : IRequest<ValidationResult>
        {
            public Post Post { get; }
            public string GroupId { get; set; }
            public ClaimsPrincipal User { get; }

            public Query(NewPostInputModel model, ClaimsPrincipal user)
            {
                User = user;
                GroupId = model.GroupId;
                Post = new Post
                {
                    Body = model.Body,
                    Title = model.Title,
                    CreatedAt = DateTime.Now
                };
            }
        }

        public class Handler : IRequestHandler<Query, ValidationResult>
        {
            private readonly IActorSystemService _actorSystemService;
            private readonly IFirestoreService _firestoreService;

            public Handler(IActorSystemService actorSystemService, IFirestoreService firestoreService)
            {
                _actorSystemService = actorSystemService;
                _firestoreService = firestoreService;
            }

            public async Task<ValidationResult> Handle(Query query, CancellationToken cancellationToken)
            {
                // Verify
                var user = await _firestoreService.GetUserAsync(query.User, cancellationToken);

                if (user == null)
                    return new ValidationResult(false, "Failed to validate user request");

                var transactionId = Guid.NewGuid();
                _actorSystemService.SagaManager.Tell(new ExecuteAddPostSagaCommand
                {
                    SagaName = nameof(CreatePostSagaActor),
                    TransactionId = transactionId,
                    GroupId = query.GroupId,
                    Post = query.Post,
                    User = user
                });

                return new ValidationResult(true, transactionId.ToString());
            }
        }
    }
}