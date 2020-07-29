using Akka.Actor;
using OpenSpark.Discussions.Indexes;
using OpenSpark.Shared;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using Raven.Client.Documents.Linq;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Domain;

namespace OpenSpark.Discussions.Actors
{
    public class PostQueryActor : ReceiveActor
    {
        private const int TakeAmountPerQuery = 10;

        public PostQueryActor()
        {
            Receive<NewsFeedQuery>(query =>
            {
                var exclude = query.Seen.Select(s => s.ConvertToRavenId<Post>()).ToList();

                var posts = query.User == null
                    ? GetNonPrivateGroupPosts(exclude)
                    : GetAllGroupPosts(query.User.Groups, exclude);

                Sender.Tell(new PayloadEvent(query) { Payload = MapRequests(posts, query.User) });
            });

            Receive<GroupPostsQuery>(query =>
            {
                if (string.IsNullOrWhiteSpace(query.PostId))
                {
                    IList<GetGroupPostsIndex.Result> posts;
                    var exclude = query.Seen.Select(s => s.ConvertToRavenId<Post>()).ToList();

                    if (query.User != null && query.User.Groups.Contains(query.GroupId))
                    {
                        posts = GetAllGroupPosts(new[] { query.GroupId }, exclude);
                    }
                    else
                    {
                        posts = GetNonPrivateGroupPosts(exclude, query.GroupId);
                    }

                    Sender.Tell(new PayloadEvent(query) { Payload = MapRequests(posts, query.User) });
                }
                else
                {
                    // select only 1 post with id
                    var posts = GetGroupPost(query.GroupId, query.PostId);
                    Sender.Tell(new PayloadEvent(query) { Payload = MapRequests(posts, query.User) });
                }
            });
        }

        private static IEnumerable<GetGroupPostsIndex.Result> GetGroupPost(string groupId, string postId)
        {
            var ravenPostId = postId.ConvertToRavenId<Post>();

            using var session = DocumentStoreSingleton.Store.OpenSession();

            var result = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                .Where(d => d.GroupId == groupId && d.PostId == ravenPostId);

            return SortResults(result);
        }

        /// <summary>
        /// Gets the relevant posts for the given authenticated user.
        /// </summary>
        private static IList<GetGroupPostsIndex.Result> GetAllGroupPosts(
            IEnumerable<string> groups, 
            IReadOnlyCollection<string> exclude)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var result = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                .Where(d => d.GroupId.In(groups) && !d.PostId.In(exclude));

            return SortResults(result);
        }

        /// <summary>
        /// Gets the most popular posts for the connected, unauthenticated user.
        /// </summary>
        private static IList<GetGroupPostsIndex.Result> GetNonPrivateGroupPosts(
            IReadOnlyCollection<string> exclude, 
            string groupId = null)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();
            IRavenQueryable<GetGroupPostsIndex.Result> query;

            if (groupId == null)
            {
                query = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                    .Where(d => !d.IsPrivate && !d.PostId.In(exclude));
            }
            else
            {
                query = session.Query<GetGroupPostsIndex.Result, GetGroupPostsIndex>()
                    .Where(d => d.GroupId == groupId && !d.IsPrivate && !d.PostId.In(exclude));
            }

            return SortResults(query);
        }

        private static IList<PostViewModel> MapRequests(IEnumerable<GetGroupPostsIndex.Result> results, User user)
        {
            var posts = results.Select(r => new PostViewModel
            {
                GroupId = r.GroupId,
                AuthorUserId = r.AuthorUserId,
                TotalComments = r.TotalComments,
                Body = r.Body,
                When = r.CreatedAt.ToTimeAgoFormat(),
                Title = r.Title,
                PostId = r.PostId.ConvertToEntityId(),
                UpVotes = r.Votes.Count(v => v.Up),
                DownVotes = r.Votes.Count(v => v.Down),
                VotedUp = user != null && r.Votes.Any(v => v.UserId == user.AuthUserId && v.Up),
                VotedDown = user != null && r.Votes.Any(v => v.UserId == user.AuthUserId && v.Down)
            }).ToList();

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