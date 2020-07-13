using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Configuration;
using OpenSpark.Discussions.Indexes;
using OpenSpark.Domain;
using OpenSpark.Shared.Commands;
using OpenSpark.Shared.Payloads;
using OpenSpark.Shared.ViewModels;
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
                    Posts = MapRequests(posts)
                };

                // This will go to the callback actor
                Sender.Tell(payload);

//                var webApi = ActorSystem.Create("WebApiSystem", config);
//                webApi.ActorSelection("akka.tcp://WebApiSystem@localhost:9091/user/Callback").Tell(payload);

                Self.GracefulStop(TimeSpan.FromSeconds(5));
            });
        }

        /// <summary>
        /// Gets the relevant posts for the given authenticated user.
        /// </summary>
        private IEnumerable<GetGroupPosts.Result> GetUserNewsFeed()
        {
            using var session = DatabaseSingleton.Store.OpenSession();

            var query = session.Query<GetGroupPosts.Result, GetGroupPosts>()
                .Where(d => d.GroupId.In(_user.Groups));

            return SortResults(query);
        }

        /// <summary>
        /// Gets the most popular posts for the connected, unauthenticated user.
        /// </summary>
        private static IEnumerable<GetGroupPosts.Result> GetMostPopularPosts()
        {
            using var session = DatabaseSingleton.Store.OpenSession();

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
                When = ConvertDateTimeToPostFriendlyFormat(r.CreatedAt),
                Title = r.Title,
                Id = r.PostId
            }).ToList();
        

        private static IEnumerable<GetGroupPosts.Result> SortResults(IRavenQueryable<GetGroupPosts.Result> query) => query
            .Take(10)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.TotalComments)
            .ThenByDescending(p => p.Votes)
            .ToList();

        private static string ConvertDateTimeToPostFriendlyFormat(DateTime dateTime)
        {
            const int secondsPerMinute = 60;
            const int secondsPerHour = 60 * secondsPerMinute;
            const int secondsPerDay = 24 * secondsPerHour;
            const int secondsPerMonth = 30 * secondsPerDay;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            var delta = Math.Abs(ts.TotalSeconds);

            if (delta < secondsPerMinute)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * secondsPerMinute)
                return "a minute ago";

            if (delta < 60 * secondsPerMinute)
                return ts.Minutes + " minutes ago";

            if (delta < 120 * secondsPerMinute)
                return "an hour ago";

            if (delta < 24 * secondsPerHour)
                return ts.Hours + " hours ago";

            if (delta < 48 * secondsPerHour)
                return "yesterday";

            if (delta < 30 * secondsPerDay)
                return ts.Days + " days ago";

            if (delta < 12 * secondsPerMonth)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
          
            var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }
}