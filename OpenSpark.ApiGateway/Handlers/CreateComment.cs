using MediatR;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Handlers
{
    public class CreateComment
    {
        public class Query : IRequest<ValidationResult>
        {
            public CommentInputModel Model { get; }

            public Query(CommentInputModel model)
            {
                Model = model;
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

                var remoteCommand = new CreateCommentCommand
                {
                    User = _user,
                    Body = query.Model.Body,
                    PostId = query.Model.PostId,
                };

                _actorSystemService.SendRemoteCommand(remoteCommand, RemoteSystem.Discussions);

                return Task.FromResult(ValidationResult.Success);
            }
        }
    }
}