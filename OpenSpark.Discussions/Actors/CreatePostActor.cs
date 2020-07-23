using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Routing;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands.Posts;
using OpenSpark.Shared.Events.CreatePost;
using OpenSpark.Shared.RavenDb;

namespace OpenSpark.Discussions.Actors
{
    public class CreatePostActor : ReceiveActor
    {
        public CreatePostActor()
        {
            Receive<CreatePostCommand>(command =>
            {
                using var session = DocumentStoreSingleton.Store.OpenSession();

                var post = new Post
                {
                    Id = session.GenerateRavenId<Post>(),
                    Title = command.Title,
                    Body = command.Body,
                    CreatedAt = DateTime.Now,
                    Votes = 1,
                    AuthorUserId = command.User.AuthUserId,
                    Comments = new List<Comment>()
                };

                session.Store(post);
                session.SaveChanges();

                Sender.Tell(new PostCreatedEvent
                {
                    PostId = post.Id
                });
            });
        }

        public static Props Props { get; } = Props.Create<CreatePostActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));
    }
}