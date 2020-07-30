using Akka.Actor;
using Akka.Routing;
using OpenSpark.Shared.Events.CreatePost;
using OpenSpark.Shared.RavenDb;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Discussions.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Commands.Discussions;
using OpenSpark.Shared.Domain;

namespace OpenSpark.Discussions.Actors
{
    public class CreatePostActor : ReceiveActor
    {
        public static Props Props { get; } = Props.Create<CreatePostActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));

        public CreatePostActor()
        {
            Receive<CreatePostCommand>(command =>
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

                var post = new Post
                {
                    Id = session.GenerateRavenId<Post>(),
                    Title = command.Title,
                    Body = command.Body,
                    CreatedAt = DateTime.Now,
                    Votes = votes,
                    AuthorUserId = command.User.AuthUserId,
                    Comments = new List<Comment>()
                };

                var groupPosts = session.Query<GroupPosts>()
                    .FirstOrDefault(d => d.GroupId == command.GroupId);

                if (groupPosts != null)
                {
                    groupPosts.Posts.Add(post);
                }
                else
                {
                    groupPosts = new GroupPosts
                    {
                        GroupId = command.GroupId,
                        Posts = new List<Post> { post },
                        IsPrivate = command.GroupVisibility == VisibilityStatus.Private,
                    };

                    session.Store(groupPosts);
                }

                session.Store(groupPosts);
                session.SaveChanges();

                Sender.Tell(new PostCreatedEvent
                {
                    PostId = post.Id.ConvertToClientId()
                });
            });
        }
    }
}