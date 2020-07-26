using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Routing;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.RavenDb;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.Discussions.Actors
{
    public class CommentsActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<CommentsActor>()
            .WithRouter(new RoundRobinPool(2,
                new DefaultResizer(1, 10)));

        public CommentsActor()
        {
            Receive<CreateCommentCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                session.Store(new Comment
                {
                    Id = session.GenerateRavenId<Comment>(),
                    CreatedAt = DateTime.Now,
                    AuthorUserId = command.User.AuthUserId,
                    Votes = 1,
                    Body = command.Body,
                    PostId = command.PostId.ConvertToRavenId<Post>()
                });

                session.SaveChanges();
            });

            Receive<CommentsQuery>(query =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var ravenPostId = query.PostId.ConvertToRavenId<Post>();

                var comments = session.Query<Comment>().Where(c => c.PostId == ravenPostId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Votes)
                    .ToList();

                var payload = new PayloadEvent(query)
                {
                    Payload = comments.Select(c => new CommentViewModel
                    {
                        When = c.CreatedAt.ToTimeAgoFormat(),
                        CommentId = c.Id.ConvertToEntityId(),
                        PostId = c.PostId.ConvertToEntityId(),
                        Body = c.Body,
                        AuthorUserId = c.AuthorUserId,
                        Votes = c.Votes
                    }).ToList()
                };

                Sender.Tell(payload);
            });
        }
    }
}
