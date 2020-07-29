using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.ApiGateway.Handlers.Queries
{
    public class FetchComments
    {
        public class Query : IRequest<Unit>
        {
            public string PostId { get; }
            public string ConnectionId { get; }
            public string Callback { get; }

            public Query(string connectionId, string callback, string postId)
            {
                ConnectionId = connectionId;
                Callback = callback;
                PostId = postId;
            }
        }

        public class Handler : IRequestHandler<Query, Unit>
        {
            private readonly IActorSystem _actorSystem;
            private readonly IMessageContextBuilder _builder;
            private readonly IFirestoreService _firestoreService;

            public Handler(
                IActorSystem actorSystem,
                IMessageContextBuilder builder,
                IFirestoreService firestoreService)
            {
                _actorSystem = actorSystem;
                _builder = builder;
                _firestoreService = firestoreService;
            }

            public Task<Unit> Handle(Query query, CancellationToken cancellationToken)
            {
                var remoteQuery = new CommentsQuery
                {
                    PostId = query.PostId,
                    Seen = new List<string>(0)
                };

                var context = _builder.CreateQueryContext(remoteQuery)
                    .SetClientCallback(query.Callback, query.ConnectionId)
                    .OnPayloadReceived(OnPayloadReceived)
                    .ForRemoteSystem(RemoteSystem.Discussions)
                    .Build();

                _actorSystem.SendQuery(context);

                return Unit.Task;
            }

            public void OnPayloadReceived(PayloadEvent @event)
            {
                if (!(@event.Payload is List<CommentViewModel> payload)) return;

                foreach (var model in payload)
                {
                    var author = _firestoreService.GetUserAsync(model.AuthorUserId).Result;
                    model.AuthorUserId = null;
                    model.AuthorDisplayName = author.DisplayName;
                }
            }
        }
    }
}