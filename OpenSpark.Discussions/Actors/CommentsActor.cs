using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Discussions;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.RavenDb;
using OpenSpark.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Discussions.Domain;

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

                var votes = new List<Vote>
                {
                    new Vote
                    {
                        UserId = command.User.AuthUserId,
                        Up = true
                    }
                };

                var comment = new Comment
                {
                    Id = session.GenerateRavenId<Comment>(),
                    CreatedAt = DateTime.Now,
                    AuthorUserId = command.User.AuthUserId,
                    Votes = votes,
                    Body = command.Body,
                    PostId = command.PostId.ConvertToRavenId<Post>()
                };

                session.Store(comment);
                session.SaveChanges();
                Sender.Tell(comment.Id.ConvertToClientId());
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
                        CreatedAt = c.CreatedAt.ToTimeAgoFormat(),
                        CommentId = c.Id.ConvertToClientId(),
                        PostId = c.PostId.ConvertToClientId(),
                        Body = c.Body,
                        AuthorUserId = c.AuthorUserId,
                        UpVotes = c.Votes.Count(v => v.Up),
                        DownVotes = c.Votes.Count(v => v.Down),
                        VotedUp = c.Votes.Any(v => v.UserId == query.User.AuthUserId && v.Up),
                        VotedDown = c.Votes.Any(v => v.UserId == query.User.AuthUserId && v.Down)
                    }).ToList()
                };

                Sender.Tell(payload);
            });

            Receive<ChangeVoteCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var ravenPostId = command.PostId.ConvertToRavenId<Post>();

                if (string.IsNullOrWhiteSpace(command.CommentId))
                {
                    // changing post vote
                    var post = session.Load<Post>(ravenPostId);
                    if (post == null) return;

                    post.Votes.RemoveAll(v => v.UserId == command.User.AuthUserId);

                    var newVote = GetNewVote(command.Amount, command.User.AuthUserId);
                    if (newVote != null) post.Votes.Add(newVote);

                    session.Store(post);
                }
                else
                {
                    var ravenCommentId = command.CommentId.ConvertToRavenId<Post>();

                    // changing comment vote
                    var comment = session.Query<Post>()
                        .Where(p => p.Id == ravenPostId)
                        .Select(p => p.Comments.FirstOrDefault(c => c.Id == ravenCommentId))
                        .FirstOrDefault();

                    if (comment == null) return;

                    var newVote = GetNewVote(command.Amount, command.User.AuthUserId);
                    if (newVote != null) comment.Votes.Add(newVote);

                    session.Store(comment);
                }

                session.SaveChanges();
            });
        }

        private Vote GetNewVote(int amount, string voter) => amount switch
        {
            1 => new Vote { UserId = voter, Up = true },
            -1 => new Vote { UserId = voter, Up = true },
            _ => null
        };
    }
}