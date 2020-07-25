using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Akka.Actor;
using OpenSpark.ApiGateway.Builders;
using OpenSpark.ApiGateway.Services;
using OpenSpark.ApiGateway.StateData;
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

            // We have the posts, now iterate and send queries for each name and fill
            var posts = StateData.Received.Values
                .Where(s => s.Payload is List<PostViewModel>)
                .Select(s => s.Payload)
                .Cast<List<PostViewModel>>()
                .Single();

            var nextQueries = new List<QueryContext>();

            foreach (var post in posts)
            {
                // Get Author Display Name
                var author = _firestoreService.GetUserAsync(post.AuthorUserId, CancellationToken.None).Result;
                post.AuthorDisplayName = author.DisplayName;

                // Create queries to get group names
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
            }

            // Execute group name queries
            foreach (var queryContext in nextQueries)
                ActorSystemService.SendRemoteQuery(queryContext, Self);

            // Wait for payload events
            var pending = GetPendingQueries(nextQueries);
            Stay().Using(new MultiQueryStateData(StateData.Received, pending));
        }
    }
}