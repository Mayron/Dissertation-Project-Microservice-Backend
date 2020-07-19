using Akka.Actor;
using OpenSpark.Discussions.Indexes;
using OpenSpark.Domain;
using OpenSpark.Shared.Events.Payloads;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;
using Raven.Client.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenSpark.Shared;

namespace OpenSpark.Discussions.Actors
{
    public class NewsFeedActor : ReceiveActor
    {
        private readonly User _user;

        public NewsFeedActor(User user)
        {
            _user = user;

            Receive<NewsFeedQuery>(query =>
            {
                var posts = _user == null ? GetMostPopularPosts() : GetUserNewsFeed();

                var payload = new PayloadEvent
                {
                    ConnectionId = query.ConnectionId,
                    Callback = query.Callback,
                    Payload = MapRequests(posts)
                };

                // This will go to the callback actor
                Sender.Tell(payload);

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        /// <summary>
        /// Gets the relevant posts for the given authenticated user.
        /// </summary>
        private IEnumerable<GetGroupPosts.Result> GetUserNewsFeed()
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var query = session.Query<GetGroupPosts.Result, GetGroupPosts>()
                .Where(d => d.GroupId.In(_user.Groups));

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

       
    }
}