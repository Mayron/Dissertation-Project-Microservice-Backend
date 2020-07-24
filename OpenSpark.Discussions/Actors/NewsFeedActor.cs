using Akka.Actor;
using Akka.Routing;
using OpenSpark.Discussions.Indexes;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using Raven.Client.Documents.Linq;
using System.Collections.Generic;
using System.Linq;
using Group = OpenSpark.Domain.Group;

namespace OpenSpark.Discussions.Actors
{
    public class NewsFeedActor : ReceiveActor
    {
        public NewsFeedActor()
        {
            Receive<NewsFeedQuery>(query =>
            {
                var posts = query.User == null
                    ? GetMostPopularPosts()
                    : GetUserNewsFeed(query.User.Groups);

                Sender.Tell(new PayloadEvent(query) { Payload = MapRequests(posts) });
            });
        }

        /// <summary>
        /// Gets the relevant posts for the given authenticated user.
        /// </summary>
        private static IEnumerable<GetGroupPosts.Result> GetUserNewsFeed(IEnumerable<string> groups)
        {
            var ravenGroupIds = groups.Select(g => g.ConvertToRavenId<Group>());
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var query = session.Query<GetGroupPosts.Result, GetGroupPosts>()
                .Where(d => d.GroupId.In(ravenGroupIds));

            return SortResults(query);
        }

        /// <summary>
        /// Gets the most popular posts for the connected, unauthenticated user.
        /// </summary>
        private static IEnumerable<GetGroupPosts.Result> GetMostPopularPosts()
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var query = session.Query<GetGroupPosts.Result, GetGroupPosts>()
                .Where(d => d.IsPublic);

            return SortResults(query);
        }

        private static IList<PostViewModel> MapRequests(IEnumerable<GetGroupPosts.Result> results) =>
            results.Select(r => new PostViewModel
            {
                AuthorUserId = r.AuthorUserId,
                TotalComments = r.TotalComments,
                Votes = r.Votes,
                Body = r.Body,
                When = r.CreatedAt.ConvertToHumanFriendlyFormat(),
                Title = r.Title,
                Id = r.PostId
            }).ToList();

        private static IEnumerable<GetGroupPosts.Result> SortResults(IRavenQueryable<GetGroupPosts.Result> query) => query
            .Take(10)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.TotalComments)
            .ThenByDescending(p => p.Votes)
            .ToList();

        public static Props Props { get; } = Props.Create<NewsFeedActor>()
            .WithRouter(new RoundRobinPool(5,
                new DefaultResizer(1, 10)));
    }
}