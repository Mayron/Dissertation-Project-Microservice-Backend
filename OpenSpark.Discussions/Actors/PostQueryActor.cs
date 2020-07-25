using Akka.Actor;
using OpenSpark.Discussions.Indexes;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using Raven.Client.Documents.Linq;
using System.Collections.Generic;
using System.Linq;

namespace OpenSpark.Discussions.Actors
{
    public class PostQueryActor : ReceiveActor
    {
        private const int TakeAmountPerQuery = 10;
        private readonly List<string> _viewed;

        public PostQueryActor()
        {
            _viewed = new List<string>(TakeAmountPerQuery);

            Receive<NewsFeedQuery>(query =>
            {
                var posts = query.User == null
                    ? GetNonPrivateGroupPosts()
                    : GetAllGroupPosts(query.User.Groups);

                Sender.Tell(new PayloadEvent(query) { Payload = MapRequests(posts) });
            });

            Receive<GroupPostsQuery>(query =>
            {
                IList<GetGroupPostsIndex.Result> posts;

                if (query.User != null && query.User.Groups.Contains(query.GroupId))
                {
                    posts = GetAllGroupPosts(new[] { query.GroupId });
                }
                else
                {
                    posts = GetNonPrivateGroupPosts(query.GroupId);
                }

                Sender.Tell(new PayloadEvent(query) { Payload = MapRequests(posts) });
            });
        }

        /// <summary>
        /// Gets the relevant posts for the given authenticated user.
        /// </summary>
        private IList<GetGroupPostsIndex.Result> GetAllGroupPosts(IEnumerable<string> groups)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var result = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                .Where(d => d.GroupId.In(groups) && !d.PostId.In(_viewed));

            return SortResults(result);
        }

        /// <summary>
        /// Gets the most popular posts for the connected, unauthenticated user.
        /// </summary>
        private IList<GetGroupPostsIndex.Result> GetNonPrivateGroupPosts(string groupId = null)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();
            IRavenQueryable<GetGroupPostsIndex.Result> query;

            if (groupId == null)
            {
                query = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                    .Where(d => !d.IsPrivate && !d.PostId.In(_viewed));
            }
            else
            {
                query = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                    .Where(d => d.GroupId == groupId && !d.IsPrivate && !d.PostId.In(_viewed));
            }

            return SortResults(query);
        }

        private IList<PostViewModel> MapRequests(IEnumerable<GetGroupPostsIndex.Result> results)
        {
            var posts = results.Select(r => new PostViewModel
            {
                GroupId = r.GroupId,
                AuthorUserId = r.AuthorUserId,
                TotalComments = r.TotalComments,
                Votes = r.Votes,
                Body = r.Body,
                When = r.CreatedAt.ToUniversalTime().ToLongDateString(),
                Title = r.Title,
                PostId = r.PostId
            }).ToList();

            foreach (var post in posts) _viewed.Add(post.PostId);
            return posts;
        }

        private static IList<GetGroupPostsIndex.Result> SortResults(
            IRavenQueryable<GetGroupPostsIndex.Result> ravenQueryable) => ravenQueryable
            .Take(TakeAmountPerQuery)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.TotalComments)
            .ThenByDescending(p => p.Votes)
            .ToList();
    }
}