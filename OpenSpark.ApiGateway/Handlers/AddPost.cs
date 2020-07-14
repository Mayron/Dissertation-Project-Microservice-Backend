using System;
using System.Security.Claims;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Models;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Handlers
{
    public class AddPost
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
                    AuthorUserId = model.AuthorUserId,
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

                _actorSystemService.StartSaga(new CreateUserPostRequestCommand
                {
                    TransactionId = Guid.NewGuid(),
                    GroupId = query.GroupId,
                    Post = query.Post,
                    User = user
                });

                return ValidationResult.Success;
            }
        }
    }
}