using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.InputModels;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands;

namespace OpenSpark.ApiGateway.Handlers
{
    public class NewPost
    {
        public class Query : IRequest
        {
            public Post Post { get; }
            public string GroupId { get; set; }

            public Query(NewPostInputModel model)
            {
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

        public class Handler : IRequestHandler<Query>
        {
            private readonly IRemoteActorSystemService _remoteActorSystemService;

            public Handler(IRemoteActorSystemService remoteActorSystemService)
            {
                _remoteActorSystemService = remoteActorSystemService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                _remoteActorSystemService.Send(new AddPostCommand
                {
                    GroupId = query.GroupId,
                    Post = query.Post
                });

                return Unit.Task;
            }
        }
    }
}