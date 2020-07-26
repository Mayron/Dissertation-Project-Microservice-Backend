using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
using OpenSpark.Domain;
using OpenSpark.Shared;
using OpenSpark.Shared.Queries;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.ApiGateway.Actors.MultiQueryHandlers
{
    public class GetPostsMultiQueryHandler : MultiQueryParallelHandler
    {
        private readonly IFirestoreService _firestoreService;

        private const string GettingNamesState = "GettingNames";

        public GetPostsMultiQueryHandler(
            MultiQueryContext context,
            IActorRef aggregator,
            IActorSystemService actorSystemService,
            IFirestoreService firestoreService) : base(context, aggregator, actorSystemService)
        {
            _firestoreService = firestoreService;

            SetNextState(GettingNamesState);
            OnTransition(OnPreGettingNamesState);
        }

        private void OnPreGettingNamesState(string initialState, string nextState)
        {
            if (initialState != InitialState || nextState != GettingNamesState) return;

            var received = NextStateData.Received;

            // We have the posts, now iterate and send queries for each name and fill
            var posts = received.Values
                .Where(s => s.Payload is List<PostViewModel>)
                .Select(s => s.Payload)
                .Cast<List<PostViewModel>>()
                .Single();

            var nextQueries = new List<QueryContext>();
            var authors = new Dictionary<string, string>();
            var groupIds = new List<string>();

            foreach (var post in posts)
            {
                // Get Author Display Name
                if (authors.ContainsKey(post.AuthorUserId))
                {
                    post.AuthorDisplayName = authors[post.AuthorUserId];
                }
                else
                {
                    var author = _firestoreService.GetUserAsync(post.AuthorUserId, CancellationToken.None).Result;
                    post.AuthorDisplayName = author.DisplayName;
                    authors[post.AuthorUserId] = author.DisplayName;
                }

                post.AuthorUserId = null; // do not send this to the client

                // Create queries to get group names for unique groups only (don't send duplicate queries)
                if (groupIds.Contains(post.GroupId)) continue;

                var remoteQuery = new GroupDetailsQuery
                {
                    GroupId = post.GroupId,
                    RetrieveGroupNameOnly = true
                };

                var builder = new QueryContextBuilder(remoteQuery, User);
                var queryContext = builder.SetMultiQueryId(MultiQueryId)
                    .ForRemoteSystem(RemoteSystem.Groups)
                    .Build();

                nextQueries.Add(queryContext);
                groupIds.Add(post.GroupId);
            }

            // Execute group name queries
            foreach (var queryContext in nextQueries)
                ActorSystemService.SendRemoteQuery(queryContext, Self);

            // Wait for payload events
            var pending = GetPendingQueries(nextQueries);

            if (pending.Count == 0) Context.Stop(Self);

            NextStateData.Pending = pending;
        }
    }
}