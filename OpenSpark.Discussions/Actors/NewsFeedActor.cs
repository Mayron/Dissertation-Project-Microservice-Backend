using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using OpenSpark.Discussions.Commands;
using OpenSpark.Discussions.Indexes;
using OpenSpark.Discussions.Payloads;
using OpenSpark.Domain;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace OpenSpark.Discussions.Actors
{
    public class NewsFeedActor : ReceiveActor
    {
        private readonly User _user;

        public NewsFeedActor(User user)
        {
            _user = user;

            Receive<FetchNewsFeedCommand>(command =>
            {
                var posts = _user == null ? GetMostPopularPosts() : GetUserNewsFeed();

                var payload = new NewsFeedPostsPayload
                {
                    ConnectionId = command.ConnectionId,
                    Posts = posts
                };

                // This will go to the callback actor
                Sender.Tell(payload);

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        /// <summary>
        /// Gets the relevant posts for the given authenticated user.
        /// </summary>
        private List<Post> GetUserNewsFeed()
        {
            using var session = DatabaseSingleton.Store.OpenSession();

            var query = session.Query<GetPostsFromDiscussionArea.Result, GetPostsFromDiscussionArea>()
                .Where(d => d.AreaId.In(_user.Projects) || d.AreaId.In(_user.Groups));

            return ApplySorting(query);
        }

        /// <summary>
        /// Gets the most popular posts for the connected, unauthenticated user.
        /// </summary>
        private static List<Post> GetMostPopularPosts()
        {
            try
            {
                using var session = DatabaseSingleton.Store.OpenSession();

                var query = session.Query<GetPostsFromDiscussionArea.Result, GetPostsFromDiscussionArea>()
                    .Where(d => d.IsPublic);

                return ApplySorting(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured when retrieving from RavenDB: {ex}");
            }

            return new List<Post>(0);
        }

        private static List<Post> ApplySorting(IRavenQueryable<GetPostsFromDiscussionArea.Result> query) => query
            .Select(r => r.Post)
            .Take(10)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Comments.Count)
            .ThenByDescending(p => p.Votes)
            .ToList();
    }
}
